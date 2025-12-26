# Implementation Plan: MQ Patterns Playground

**Branch**: `001-mq-patterns-playground` | **Date**: 2025-12-26 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-mq-patterns-playground/spec.md`

## Summary

Build an interactive single-page web application demonstrating three IBM MQ messaging
patterns (Point-to-Point, Publish/Subscribe, Request/Reply) with real-time visualization.
The application uses a .NET 8 backend with SignalR for real-time updates, Vue.js 2
frontend with a 3-column layout, and IBM MQ running in Docker for message brokering.

## Technical Context

**Language/Version**: C# (.NET 8), JavaScript (Vue.js 2)
**Primary Dependencies**: ASP.NET Core Web API, SignalR, IBM.XMS.NETCore, Vue.js 2 (Options API)
**Storage**: In-memory only (session-based, no database)
**Testing**: xUnit (.NET), Vue Test Utils (frontend) - minimal, critical paths only
**Target Platform**: Docker containers, modern browsers (Chrome, Firefox, Edge)
**Project Type**: Web application (frontend + backend)
**Performance Goals**: <2 second message round-trip visibility in UI
**Constraints**: Single user, local development only, plain text messages
**Scale/Scope**: 3 messaging patterns, 50 messages per panel session limit

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Status |
|-----------|-------------|--------|
| I. Simplicity First | Start with simplest implementation; no over-engineering | ✅ PASS - Single solution with minimal abstractions |
| I. Simplicity First | YAGNI - no speculative features | ✅ PASS - Only implementing required 3 patterns |
| II. Production Quality | XML docs on public C# APIs | ✅ WILL COMPLY |
| II. Production Quality | Vue component prop/emit documentation | ✅ WILL COMPLY |
| II. Production Quality | Explicit error handling, logging for MQ flows | ✅ WILL COMPLY |
| III. Educational Clarity | Producers/consumers in distinct namespaces | ✅ WILL COMPLY |
| III. Educational Clarity | Comments explain "why" of patterns | ✅ WILL COMPLY |
| III. Educational Clarity | MQ config externalized and documented | ✅ WILL COMPLY |
| III. Educational Clarity | SignalR hubs indicate which MQ events relayed | ✅ WILL COMPLY |
| Technology Stack | .NET 8, Vue.js 2, IBM MQ Docker, SignalR | ✅ ALIGNED |
| Deployment Strategy | Docker Compose single command startup | ✅ WILL COMPLY |
| Deployment Strategy | .env.example for environment variables | ✅ WILL COMPLY |

**Gate Status**: ✅ PASSED - No violations. Proceed to Phase 0.

## Project Structure

### Documentation (this feature)

```text
specs/001-mq-patterns-playground/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (OpenAPI specs)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
backend/
├── src/
│   └── MqPlayground.Api/
│       ├── Program.cs
│       ├── Hubs/
│       │   └── MessageHub.cs           # SignalR hub for real-time updates
│       ├── Services/
│       │   ├── IMqConnectionService.cs
│       │   ├── MqConnectionService.cs  # IBM MQ connection management
│       │   ├── PointToPointService.cs  # P2P pattern implementation
│       │   ├── PubSubService.cs        # Pub/Sub pattern implementation
│       │   └── RequestReplyService.cs  # Request/Reply pattern implementation
│       ├── Models/
│       │   ├── Message.cs
│       │   ├── ConnectionStatus.cs
│       │   └── PatternConfig.cs
│       └── Workers/
│           ├── PointToPointConsumer.cs # Background consumer for P2P
│           ├── TopicSubscriber.cs      # Background subscriber for Pub/Sub
│           └── ReplyHandler.cs         # Background responder for Request/Reply
└── tests/
    └── MqPlayground.Api.Tests/

frontend/
├── src/
│   ├── App.vue
│   ├── main.js
│   ├── components/
│   │   ├── PatternPanel.vue           # Reusable panel wrapper
│   │   ├── MessageInput.vue           # Text input + Send button
│   │   ├── MessageList.vue            # Displays message history
│   │   ├── ConnectionStatus.vue       # Shows MQ connection state
│   │   └── SubscriberControl.vue      # Add/Remove subscriber buttons
│   ├── panels/
│   │   ├── PointToPointPanel.vue      # P2P pattern UI
│   │   ├── PubSubPanel.vue            # Pub/Sub pattern UI
│   │   └── RequestReplyPanel.vue      # Request/Reply pattern UI
│   └── services/
│       └── signalrService.js          # SignalR client connection
└── tests/

docker/
├── docker-compose.yml                  # Orchestrates all services
├── mq/
│   └── config/                         # MQ queue/topic definitions
└── .env.example                        # Environment variable template
```

**Structure Decision**: Web application structure with separate `backend/` and `frontend/`
directories. This aligns with the constitution's requirement for clear separation between
concerns and allows independent development/testing of each layer.

## Complexity Tracking

> No violations to justify. Design follows Simplicity First principle.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | N/A | N/A |
