# Feature Specification: MQ Patterns Playground

**Feature Branch**: `001-mq-patterns-playground`
**Created**: 2025-12-26
**Status**: Draft
**Input**: User description: "MQ Patterns Playground - A complete interactive demo showing
three IBM MQ messaging patterns: Point-to-Point, Publish/Subscribe, and Request/Reply with
real-time visualization via SignalR"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Point-to-Point Messaging Demo (Priority: P1)

As a developer learning IBM MQ, I want to see a visual demonstration of Point-to-Point
messaging where I can send a message from a producer to a single consumer and watch the
message flow in real-time.

**Why this priority**: Point-to-Point is the most fundamental MQ pattern. Understanding
this pattern is a prerequisite for the more complex patterns. It delivers immediate
educational value with the simplest implementation.

**Independent Test**: Can be fully tested by opening the Point-to-Point panel, typing a
message, clicking send, and observing the message appear in the consumer panel within
2 seconds. Delivers core educational value of seeing a message traverse a queue.

**Acceptance Scenarios**:

1. **Given** the Point-to-Point panel is visible with producer and consumer sections,
   **When** I type "Hello MQ" in the message input and click Send,
   **Then** I see a visual indicator of the message entering the queue, followed by
   the message appearing in the consumer section with its content displayed.

2. **Given** I have sent multiple messages,
   **When** I view the consumer section,
   **Then** I see messages displayed in the order they were received (FIFO).

3. **Given** the MQ service is unavailable,
   **When** I attempt to send a message,
   **Then** I see a clear error message indicating the connection issue.

---

### User Story 2 - Publish/Subscribe Pattern Demo (Priority: P2)

As a developer learning IBM MQ, I want to see a visual demonstration of the
Publish/Subscribe pattern where I can publish a message to a topic and watch multiple
subscribers receive the same message simultaneously.

**Why this priority**: Pub/Sub builds on Point-to-Point concepts and demonstrates
broadcast messaging, which is essential for event-driven architectures. Requires P1
infrastructure.

**Independent Test**: Can be tested by opening the Pub/Sub panel, subscribing with
2 or more subscriber instances, publishing a message, and observing all subscribers
receive the same message simultaneously.

**Acceptance Scenarios**:

1. **Given** the Pub/Sub panel is visible with a topic publisher and two subscriber
   sections,
   **When** I publish a message "Breaking News!" to the topic,
   **Then** both subscribers display the message simultaneously in their sections.

2. **Given** a topic with three active subscribers,
   **When** I add a fourth subscriber after messages were already published,
   **Then** the fourth subscriber only receives messages published after it subscribed
   (not historical messages).

3. **Given** one subscriber disconnects,
   **When** I publish a new message,
   **Then** only the remaining connected subscribers receive the message.

---

### User Story 3 - Request/Reply Pattern Demo (Priority: P3)

As a developer learning IBM MQ, I want to see a visual demonstration of the
Request/Reply pattern where I can send a request message and see the correlated
response message flow back to the original requester.

**Why this priority**: Request/Reply is the most complex pattern, requiring correlation
IDs and temporary reply queues. It builds on P1 and P2 knowledge and completes the
learning journey.

**Independent Test**: Can be tested by opening the Request/Reply panel, sending a
request (e.g., "What is 2+2?"), and observing the response ("4") appear in the reply
section with matching correlation ID displayed.

**Acceptance Scenarios**:

1. **Given** the Request/Reply panel shows a requester and responder section,
   **When** I send a request message "Calculate: 2+2",
   **Then** I see the request flow to the responder, followed by a response "4" flowing
   back to the requester section, with the correlation ID visible on both messages.

2. **Given** I send multiple concurrent requests,
   **When** responses arrive out of order,
   **Then** each response is correctly matched to its original request via correlation ID.

3. **Given** a request is sent but no response is received within the timeout period,
   **When** the timeout expires,
   **Then** the requester section displays a timeout error for that specific request.

---

### Edge Cases

- What happens when the IBM MQ container is stopped while messages are in transit?
  The UI MUST display a connection lost indicator and queue any pending send attempts
  for retry when connection resumes.

- What happens when a user rapidly sends many messages (spam clicking)?
  The system MUST throttle message sending to a reasonable rate (e.g., 10 messages/second)
  and provide visual feedback that additional messages are queued.

- How does the system handle very long messages?
  Messages exceeding 10,000 characters MUST be truncated in the display with an option
  to view full content in a modal/expanded view.

- What happens when the browser tab loses focus during real-time updates?
  The system MUST continue receiving updates and display them when the tab regains focus.

## Requirements *(mandatory)*

### Functional Requirements

**Core Platform**

- **FR-001**: System MUST provide a single-page web interface with three distinct visual
  panels arranged in a side-by-side 3-column layout, one column per messaging pattern
  (Point-to-Point, Pub/Sub, Request/Reply), all visible simultaneously.

- **FR-002**: System MUST display real-time message flow animations showing messages
  moving between producer and consumer components.

- **FR-003**: System MUST update the UI within 2 seconds of a message being sent or
  received (real-time via SignalR).

- **FR-004**: System MUST display connection status (connected/disconnected) to the
  MQ service at all times.

**Point-to-Point Pattern**

- **FR-005**: Users MUST be able to type a text message and send it to a designated
  queue via a "Send" button.

- **FR-006**: System MUST display received messages in the consumer section showing
  message content and timestamp.

- **FR-007**: System MUST preserve message ordering (FIFO) in the display.

**Publish/Subscribe Pattern**

- **FR-008**: Users MUST be able to publish a message to a topic.

- **FR-009**: System MUST display 2 subscriber sections by default, with the ability
  for users to dynamically add or remove subscribers (range: 2-4 subscribers).

- **FR-009a**: System MUST provide "Add Subscriber" and "Remove Subscriber" controls
  in the Pub/Sub panel to manage dynamic subscriber count.

- **FR-010**: System MUST visually indicate when a subscriber is actively subscribed
  vs. disconnected.

**Request/Reply Pattern**

- **FR-011**: Users MUST be able to send a request message that expects a response.

- **FR-012**: System MUST display a simulated response for demonstration purposes
  (e.g., echo, calculation, or canned responses).

- **FR-013**: System MUST display the correlation ID linking each request to its
  response.

- **FR-014**: System MUST handle request timeout (configurable, default 30 seconds)
  and display appropriate error.

**User Experience**

- **FR-015**: System MUST provide a "Clear" button to reset messages in each panel.

- **FR-016**: System MUST persist the last 50 messages per panel during the session
  (messages cleared on page refresh).

- **FR-017**: System MUST work on modern browsers (Chrome, Firefox, Edge - latest 2
  versions).

### Key Entities

- **Message**: Represents an MQ message with content (text), timestamp (when sent),
  correlation ID (for request/reply), and direction (sent/received).

- **Queue**: A named Point-to-Point destination for messages. Has a name identifier.

- **Topic**: A named Pub/Sub destination. Subscribers receive all messages published
  after subscribing.

- **Subscriber**: Represents a topic subscription. Has a unique ID and subscription
  status (active/inactive).

- **Pattern Panel**: A UI container for one messaging pattern. Contains message
  history, input controls, and status indicators.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can successfully send and receive a Point-to-Point message within
  2 seconds of clicking Send.

- **SC-002**: Users can publish a message and see it received by all active subscribers
  within 2 seconds.

- **SC-003**: Users can send a request and receive a correlated response within 5 seconds
  (including simulated processing time).

- **SC-004**: 100% of sent messages are displayed in the appropriate consumer/subscriber
  panel (no message loss in the UI).

- **SC-005**: Users can understand each messaging pattern after 5 minutes of interaction
  without external documentation (self-explanatory UI).

- **SC-006**: The playground runs successfully on a single developer machine using
  Docker Compose with a single command.

## Assumptions

The following reasonable defaults have been assumed:

1. **Authentication**: No user authentication required; this is a local development/demo
   tool.

2. **Data Persistence**: Messages are not persisted to a database; session-only storage
   in memory is sufficient for demo purposes.

3. **Concurrent Users**: Single user experience; no need to support multiple simultaneous
   users accessing the same instance.

4. **Message Format**: Plain text messages only; no binary or structured data formats
   required for MVP.

5. **Browser Support**: Modern evergreen browsers only; no IE11 or legacy support needed.

6. **Responder Logic**: For Request/Reply demo, a simple echo or arithmetic responder
   is sufficient rather than complex business logic.

## Clarifications

### Session 2025-12-26

- Q: Should the number of Pub/Sub subscribers be fixed or dynamic? → A: Dynamic subscribers - users can add/remove subscribers (2-4 range)
- Q: How should the three pattern panels be arranged on the page? → A: Side-by-side 3-column layout, all panels visible simultaneously
