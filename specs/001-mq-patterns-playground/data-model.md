# Data Model: MQ Patterns Playground

**Date**: 2025-12-26
**Feature**: 001-mq-patterns-playground

## Overview

This document defines the data entities for the MQ Patterns Playground. All entities
are session-based (in-memory only) and not persisted to a database.

---

## Entities

### Message

Represents a message sent or received through IBM MQ.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| id | string | Unique identifier (GUID) | Required, auto-generated |
| content | string | Message text content | Required, max 10,000 chars |
| timestamp | datetime | When message was sent/received | Required, UTC |
| direction | enum | Sent or Received | Required |
| correlationId | string | Links request to reply | Optional, for Request/Reply |
| pattern | enum | Which pattern (P2P, PubSub, ReqRep) | Required |

**Direction enum values**: `Sent`, `Received`

**Pattern enum values**: `PointToPoint`, `PublishSubscribe`, `RequestReply`

---

### ConnectionStatus

Represents the current connection state to IBM MQ.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| isConnected | boolean | Whether MQ is reachable | Required |
| queueManagerName | string | Name of connected QM | Required when connected |
| lastChecked | datetime | When status was last verified | Required, UTC |
| errorMessage | string | Description if disconnected | Optional |

---

### Subscriber

Represents a topic subscriber in the Pub/Sub pattern panel.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| id | string | Unique subscriber ID (GUID) | Required, auto-generated |
| name | string | Display name (e.g., "Subscriber 1") | Required |
| isActive | boolean | Currently subscribed to topic | Required, default true |
| messages | Message[] | Messages received by this subscriber | Required, max 50 items |

**Lifecycle**:
- Created when user clicks "Add Subscriber"
- Deleted when user clicks "Remove Subscriber"
- Minimum 2 subscribers, maximum 4 subscribers

---

### PatternPanel

UI state container for each messaging pattern.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| pattern | enum | Which pattern this panel represents | Required |
| messages | Message[] | Message history for this panel | Max 50 items, FIFO eviction |
| isLoading | boolean | Whether an operation is in progress | Required |
| error | string | Current error message if any | Optional |

---

### Request (Request/Reply specific)

Tracks pending requests awaiting replies.

| Field | Type | Description | Constraints |
|-------|------|-------------|-------------|
| id | string | Request message ID | Required |
| correlationId | string | Correlation ID for matching | Required |
| content | string | Request message content | Required |
| sentAt | datetime | When request was sent | Required, UTC |
| status | enum | Pending, Completed, TimedOut | Required |
| response | Message | The reply message if received | Optional |
| timeoutSeconds | number | Timeout duration | Required, default 30 |

**Status enum values**: `Pending`, `Completed`, `TimedOut`

---

## Relationships

```text
┌─────────────────┐
│  PatternPanel   │
│  (3 instances)  │
└────────┬────────┘
         │ contains
         ▼
┌─────────────────┐      ┌─────────────────┐
│    Message[]    │◄─────│   Subscriber    │
│  (max 50 each)  │      │  (Pub/Sub only) │
└─────────────────┘      └─────────────────┘
         │
         │ Request/Reply only
         ▼
┌─────────────────┐
│    Request      │
│ (tracks pending)│
└─────────────────┘
```

---

## Validation Rules

### Message

- `content` must not be empty
- `content` must not exceed 10,000 characters
- `timestamp` must be valid UTC datetime
- `correlationId` required only when `pattern` is `RequestReply`

### Subscriber

- `name` must be unique within the Pub/Sub panel
- Minimum 2 subscribers must exist at all times
- Maximum 4 subscribers allowed
- Cannot delete subscriber while messages are being received

### PatternPanel

- `messages` array auto-evicts oldest when exceeding 50 items
- Only one operation (`isLoading=true`) allowed at a time per panel

### Request

- `timeoutSeconds` must be between 1 and 120
- `status` transitions: Pending → Completed | TimedOut (no reverse)
- `response` only set when `status` is Completed

---

## State Transitions

### Request Lifecycle

```text
[Created] ──send──► [Pending] ──reply received──► [Completed]
                        │
                        └──timeout elapsed──► [TimedOut]
```

### Subscriber Lifecycle

```text
[Created] ──subscribe──► [Active] ◄──resubscribe──► [Inactive]
                              │                          │
                              └──────unsubscribe─────────┘
                                        │
                                        ▼
                                   [Deleted]
```

---

## Notes

1. **No database persistence**: All entities exist in memory only and are cleared
   on page refresh or server restart.

2. **Session isolation**: Each browser session has independent state; there is no
   shared state between users.

3. **Message limits**: 50 messages per panel prevents memory bloat; oldest messages
   are evicted first (FIFO).

4. **Correlation IDs**: Generated by IBM MQ; the application stores them to match
   requests with replies.
