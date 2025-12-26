# Tasks: MQ Patterns Playground

**Input**: Design documents from `/specs/001-mq-patterns-playground/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/

**Tests**: Not explicitly requested in specification. Testing marked as "minimal, critical paths only" in plan.md. Test tasks are excluded.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Web app**: `backend/src/`, `frontend/src/`
- Docker configuration in `docker/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create backend project structure with `dotnet new webapi` in backend/src/MqPlayground.Api/
- [ ] T002 [P] Add NuGet packages (IBMMQDotnetClient, Microsoft.AspNetCore.SignalR) to backend/src/MqPlayground.Api/MqPlayground.Api.csproj
- [ ] T003 [P] Create frontend project with Vue CLI in frontend/ using Vue 2 template
- [ ] T004 [P] Add npm packages (@microsoft/signalr) to frontend/package.json
- [ ] T005 [P] Create docker-compose.yml in docker/ with IBM MQ service configuration
- [ ] T006 [P] Create MQ MQSC config file in docker/mq/config/20-playground.mqsc with queue/topic definitions
- [ ] T007 [P] Create .env.example in docker/ with environment variable template
- [ ] T008 [P] Create secrets/app_password.txt placeholder in docker/secrets/

**Checkpoint**: Project scaffolding complete, ready for foundational components

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Backend Models (shared across all stories)

- [ ] T009 [P] Create Message.cs model in backend/src/MqPlayground.Api/Models/Message.cs with Id, Content, Timestamp, Direction, CorrelationId, Pattern properties
- [ ] T010 [P] Create ConnectionStatus.cs model in backend/src/MqPlayground.Api/Models/ConnectionStatus.cs with IsConnected, QueueManagerName, LastChecked, ErrorMessage properties
- [ ] T011 [P] Create PatternConfig.cs model in backend/src/MqPlayground.Api/Models/PatternConfig.cs with queue/topic name constants
- [ ] T012 [P] Create ErrorResponse.cs model in backend/src/MqPlayground.Api/Models/ErrorResponse.cs

### Backend Core Services

- [ ] T013 Create IMqConnectionService.cs interface in backend/src/MqPlayground.Api/Services/IMqConnectionService.cs defining Connect, Disconnect, GetStatus, IsConnected methods
- [ ] T014 Create MqConnectionService.cs in backend/src/MqPlayground.Api/Services/MqConnectionService.cs implementing IBM MQ connection management with retry logic
- [ ] T015 Create MessageHub.cs SignalR hub in backend/src/MqPlayground.Api/Hubs/MessageHub.cs with JoinSession, LeaveSession methods and connection state tracking

### Backend Configuration

- [ ] T016 Configure Program.cs in backend/src/MqPlayground.Api/Program.cs with SignalR, CORS, MQ service registration, and API controllers
- [ ] T017 [P] Create appsettings.json in backend/src/MqPlayground.Api/appsettings.json with MQ connection settings (host, port, channel, queue manager)
- [ ] T018 [P] Create appsettings.Development.json in backend/src/MqPlayground.Api/appsettings.Development.json with local Docker MQ settings

### Frontend Core Components

- [ ] T019 Create signalrService.js in frontend/src/services/signalrService.js with connection management, auto-reconnect, event handlers
- [ ] T020 [P] Create ConnectionStatus.vue in frontend/src/components/ConnectionStatus.vue showing MQ connection state indicator
- [ ] T021 [P] Create MessageInput.vue in frontend/src/components/MessageInput.vue with text input and send button
- [ ] T022 [P] Create MessageList.vue in frontend/src/components/MessageList.vue displaying messages with content, timestamp, direction
- [ ] T023 [P] Create PatternPanel.vue in frontend/src/components/PatternPanel.vue as reusable wrapper with title, clear button, loading state

### Backend Status Endpoint

- [ ] T024 Create StatusController.cs in backend/src/MqPlayground.Api/Controllers/StatusController.cs with GET /api/status endpoint returning ConnectionStatus

### Frontend App Shell

- [ ] T025 Update App.vue in frontend/src/App.vue with 3-column layout grid, SignalR connection initialization, connection status display

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Point-to-Point Messaging Demo (Priority: P1) 🎯 MVP

**Goal**: Users can send a message to a queue and see it consumed in real-time

**Independent Test**: Type "Hello MQ" in P2P panel, click Send, observe message appear in consumer section within 2 seconds

### Backend Implementation for US1

- [ ] T026 [US1] Create PointToPointService.cs in backend/src/MqPlayground.Api/Services/PointToPointService.cs with SendMessage method that puts message to PLAYGROUND.P2P.QUEUE
- [ ] T027 [US1] Create PointToPointConsumer.cs background worker in backend/src/MqPlayground.Api/Workers/PointToPointConsumer.cs that listens on queue and pushes to SignalR hub
- [ ] T028 [US1] Create PointToPointController.cs in backend/src/MqPlayground.Api/Controllers/PointToPointController.cs with POST /api/point-to-point/send and POST /api/point-to-point/clear endpoints
- [ ] T029 [US1] Register PointToPointConsumer as hosted service in Program.cs
- [ ] T030 [US1] Add MessageReceived SignalR broadcast in PointToPointConsumer when message received from queue

### Frontend Implementation for US1

- [ ] T031 [US1] Create PointToPointPanel.vue in frontend/src/panels/PointToPointPanel.vue with producer section (MessageInput), consumer section (MessageList), and clear button
- [ ] T032 [US1] Wire PointToPointPanel to SignalR MessageReceived event for PointToPoint pattern messages
- [ ] T033 [US1] Add PointToPointPanel to App.vue in left column of 3-column layout
- [ ] T034 [US1] Add error handling in PointToPointPanel showing connection errors from API responses
- [ ] T035 [US1] Add FIFO message ordering in PointToPointPanel consumer section with max 50 messages

**Checkpoint**: Point-to-Point demo fully functional - can send and receive messages through IBM MQ

---

## Phase 4: User Story 2 - Publish/Subscribe Pattern Demo (Priority: P2)

**Goal**: Users can publish messages to a topic and see multiple subscribers receive them simultaneously

**Independent Test**: Publish "Breaking News!" with 2 subscribers active, both display the message; add 3rd subscriber, publish again, all 3 receive

### Backend Models for US2

- [ ] T036 [P] [US2] Create Subscriber.cs model in backend/src/MqPlayground.Api/Models/Subscriber.cs with Id, Name, IsActive, MessageCount properties

### Backend Implementation for US2

- [ ] T037 [US2] Create PubSubService.cs in backend/src/MqPlayground.Api/Services/PubSubService.cs with PublishMessage, GetSubscribers, AddSubscriber, RemoveSubscriber methods
- [ ] T038 [US2] Create TopicSubscriber.cs background worker in backend/src/MqPlayground.Api/Workers/TopicSubscriber.cs that manages dynamic subscriptions to PLAYGROUND.PUBSUB topic
- [ ] T039 [US2] Create PubSubController.cs in backend/src/MqPlayground.Api/Controllers/PubSubController.cs with POST /api/pubsub/publish, GET/POST /api/pubsub/subscribers, DELETE /api/pubsub/subscribers/{id}, POST /api/pubsub/clear
- [ ] T040 [US2] Register TopicSubscriber as hosted service in Program.cs
- [ ] T041 [US2] Add SubscriberMessageReceived, SubscriberAdded, SubscriberRemoved SignalR broadcasts in PubSubService/TopicSubscriber
- [ ] T042 [US2] Implement subscriber count validation (min 2, max 4) in PubSubService

### Frontend Implementation for US2

- [ ] T043 [P] [US2] Create SubscriberControl.vue in frontend/src/components/SubscriberControl.vue with Add/Remove subscriber buttons and subscriber count display
- [ ] T044 [US2] Create PubSubPanel.vue in frontend/src/panels/PubSubPanel.vue with publisher section, dynamic subscriber sections (2-4), and subscriber controls
- [ ] T045 [US2] Wire PubSubPanel to SignalR SubscriberMessageReceived, SubscriberAdded, SubscriberRemoved events
- [ ] T046 [US2] Add PubSubPanel to App.vue in center column of 3-column layout
- [ ] T047 [US2] Implement subscriber section rendering with individual message lists per subscriber
- [ ] T048 [US2] Add subscriber active/inactive visual indicator in PubSubPanel

**Checkpoint**: Pub/Sub demo fully functional - can publish to topic and see all subscribers receive messages

---

## Phase 5: User Story 3 - Request/Reply Pattern Demo (Priority: P3)

**Goal**: Users can send a request and see the correlated response with matching correlation ID

**Independent Test**: Send "Calculate: 2+2", observe request flow to responder, see response "4" with matching correlation ID

### Backend Models for US3

- [ ] T049 [P] [US3] Create Request.cs model in backend/src/MqPlayground.Api/Models/Request.cs with Id, CorrelationId, Content, SentAt, Status, Response, TimeoutSeconds properties

### Backend Implementation for US3

- [ ] T050 [US3] Create RequestReplyService.cs in backend/src/MqPlayground.Api/Services/RequestReplyService.cs with SendRequest method using correlation IDs and reply queue
- [ ] T051 [US3] Create ReplyHandler.cs background worker in backend/src/MqPlayground.Api/Workers/ReplyHandler.cs that listens on request queue, generates response, sends to reply queue
- [ ] T052 [US3] Implement simple responder logic in ReplyHandler (echo, arithmetic evaluation for "Calculate: X+Y" format)
- [ ] T053 [US3] Create RequestReplyController.cs in backend/src/MqPlayground.Api/Controllers/RequestReplyController.cs with POST /api/request-reply/send and POST /api/request-reply/clear
- [ ] T054 [US3] Register ReplyHandler as hosted service in Program.cs
- [ ] T055 [US3] Add RequestStatusChanged SignalR broadcast when reply received or timeout occurs
- [ ] T056 [US3] Implement request timeout handling (default 30s) with TimedOut status update

### Frontend Implementation for US3

- [ ] T057 [US3] Create RequestReplyPanel.vue in frontend/src/panels/RequestReplyPanel.vue with request input, pending requests list, and completed responses display
- [ ] T058 [US3] Wire RequestReplyPanel to SignalR RequestStatusChanged event
- [ ] T059 [US3] Add RequestReplyPanel to App.vue in right column of 3-column layout
- [ ] T060 [US3] Display correlation ID on both request and response messages in UI
- [ ] T061 [US3] Add timeout indicator for pending requests with countdown or elapsed time
- [ ] T062 [US3] Handle timeout errors with visual feedback in RequestReplyPanel

**Checkpoint**: Request/Reply demo fully functional - can send requests and see correlated responses

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T063 [P] Add XML documentation comments to all public C# classes in backend/src/MqPlayground.Api/
- [ ] T064 [P] Add Vue component prop/emit documentation to all .vue files in frontend/src/
- [ ] T065 Add educational comments explaining "why" of each MQ pattern in service files
- [ ] T066 [P] Add structured logging for MQ operations in all service and worker files
- [ ] T067 Implement message throttling (10 msg/sec) in frontend MessageInput component
- [ ] T068 Implement long message truncation (10,000 chars) with expand modal in MessageList
- [ ] T069 [P] Add connection recovery handling in MqConnectionService with retry and SignalR notification
- [ ] T070 Create README.md in repository root with project overview and quickstart reference
- [ ] T071 Validate all endpoints work per quickstart.md test scenarios
- [ ] T072 Final Docker Compose test: `docker compose up` brings up all services

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent of US1
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Independent of US1/US2

### Within Each User Story

- Models before services
- Services before controllers
- Backend before frontend (API must exist for UI to call)
- Workers registered before testing

### Parallel Opportunities

**Phase 1 (Setup)**:
```bash
# These can run in parallel:
T002: Add NuGet packages
T003: Create frontend project
T004: Add npm packages
T005: Create docker-compose.yml
T006: Create MQSC config
T007: Create .env.example
T008: Create secrets placeholder
```

**Phase 2 (Foundational)**:
```bash
# Models can run in parallel:
T009: Message.cs
T010: ConnectionStatus.cs
T011: PatternConfig.cs
T012: ErrorResponse.cs

# Frontend components can run in parallel:
T020: ConnectionStatus.vue
T021: MessageInput.vue
T022: MessageList.vue
T023: PatternPanel.vue
```

**After Foundational - User Stories in Parallel**:
```bash
# Three developers can work simultaneously:
Developer A: Phase 3 (US1 - Point-to-Point)
Developer B: Phase 4 (US2 - Pub/Sub)
Developer C: Phase 5 (US3 - Request/Reply)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Point-to-Point)
4. **STOP and VALIDATE**: Send a message through IBM MQ and see it appear in the UI
5. Deploy/demo if ready - this demonstrates the core MQ integration

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (P2P) → Test independently → Demo (MVP!)
3. Add User Story 2 (Pub/Sub) → Test independently → Demo
4. Add User Story 3 (Request/Reply) → Test independently → Demo
5. Add Polish phase → Production-ready

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Point-to-Point)
   - Developer B: User Story 2 (Pub/Sub)
   - Developer C: User Story 3 (Request/Reply)
3. Stories complete and integrate independently (all use shared SignalR hub and MQ connection)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- MQ queue names: PLAYGROUND.P2P.QUEUE, PLAYGROUND.REQUEST.QUEUE, PLAYGROUND.REPLY.QUEUE
- MQ topic: PLAYGROUND.PUBSUB (topic string: playground/pubsub/)
