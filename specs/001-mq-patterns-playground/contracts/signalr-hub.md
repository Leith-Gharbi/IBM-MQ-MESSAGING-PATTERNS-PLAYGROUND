# SignalR Hub Contract: MessageHub

**Hub Path**: `/messageHub`

## Overview

The MessageHub provides real-time message notifications from the backend to connected
browser clients. All MQ events (messages received, status changes) are pushed through
this hub.

---

## Server-to-Client Methods

These methods are invoked by the server and received by the client.

### MessageReceived

Notifies client when a new message is received from MQ.

```typescript
MessageReceived(message: Message): void
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| message | Message | The received message with pattern, content, timestamp |

**Triggered by**:
- Point-to-Point consumer receives message from queue
- Pub/Sub subscriber receives message from topic
- Request/Reply handler receives response

---

### SubscriberMessageReceived

Notifies client when a specific subscriber receives a message.

```typescript
SubscriberMessageReceived(subscriberId: string, message: Message): void
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| subscriberId | string | GUID of the subscriber that received the message |
| message | Message | The received message |

**Triggered by**:
- Pub/Sub topic delivers message to subscriber

---

### RequestStatusChanged

Notifies client when a request status changes (reply received or timeout).

```typescript
RequestStatusChanged(request: Request): void
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| request | Request | Updated request with status, response if completed |

**Triggered by**:
- Reply received for a pending request
- Request timeout elapsed

---

### ConnectionStatusChanged

Notifies client when MQ connection status changes.

```typescript
ConnectionStatusChanged(status: ConnectionStatus): void
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| status | ConnectionStatus | Current connection state |

**Triggered by**:
- MQ connection established
- MQ connection lost
- MQ connection recovered

---

### SubscriberAdded

Notifies client when a new subscriber is added to Pub/Sub.

```typescript
SubscriberAdded(subscriber: Subscriber): void
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| subscriber | Subscriber | The newly created subscriber |

---

### SubscriberRemoved

Notifies client when a subscriber is removed from Pub/Sub.

```typescript
SubscriberRemoved(subscriberId: string): void
```

**Parameters**:
| Name | Type | Description |
|------|------|-------------|
| subscriberId | string | GUID of the removed subscriber |

---

## Client-to-Server Methods

These methods are invoked by the client and handled by the server.

### JoinSession

Registers client for real-time updates. Called on connection start.

```typescript
JoinSession(): Promise<SessionInfo>
```

**Returns**:
| Name | Type | Description |
|------|------|-------------|
| SessionInfo | object | Initial state (subscribers, connection status) |

---

### LeaveSession

Unregisters client from updates. Called on connection close.

```typescript
LeaveSession(): Promise<void>
```

---

## Data Types

### Message

```typescript
interface Message {
  id: string;           // GUID
  content: string;      // Message text
  timestamp: string;    // ISO 8601 datetime
  direction: 'Sent' | 'Received';
  correlationId?: string;
  pattern: 'PointToPoint' | 'PublishSubscribe' | 'RequestReply';
}
```

### ConnectionStatus

```typescript
interface ConnectionStatus {
  isConnected: boolean;
  queueManagerName?: string;
  lastChecked: string;  // ISO 8601 datetime
  errorMessage?: string;
}
```

### Subscriber

```typescript
interface Subscriber {
  id: string;           // GUID
  name: string;         // Display name
  isActive: boolean;
  messageCount: number;
}
```

### Request

```typescript
interface Request {
  id: string;           // GUID
  correlationId: string;
  content: string;
  sentAt: string;       // ISO 8601 datetime
  status: 'Pending' | 'Completed' | 'TimedOut';
  response?: Message;
  timeoutSeconds: number;
}
```

### SessionInfo

```typescript
interface SessionInfo {
  connectionStatus: ConnectionStatus;
  subscribers: Subscriber[];
}
```

---

## Connection Lifecycle

1. **Connect**: Client establishes WebSocket connection to `/messageHub`
2. **Join**: Client calls `JoinSession()` to receive initial state
3. **Receive**: Server pushes updates via server-to-client methods
4. **Reconnect**: On disconnect, client uses automatic reconnect with backoff
5. **Leave**: Client calls `LeaveSession()` before intentional disconnect

---

## Error Handling

Hub methods throw `HubException` on errors:

```typescript
try {
  await connection.invoke('JoinSession');
} catch (err) {
  // err.message contains error description
}
```

Common errors:
- "MQ connection unavailable" - MQ not reachable
- "Session already active" - Duplicate JoinSession call
