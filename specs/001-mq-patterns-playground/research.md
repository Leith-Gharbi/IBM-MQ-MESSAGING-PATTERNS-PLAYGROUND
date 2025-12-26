# Research: MQ Patterns Playground

**Date**: 2025-12-26
**Feature**: 001-mq-patterns-playground

## Decision Summary

| Area | Decision | Rationale |
|------|----------|-----------|
| MQ Client | IBMMQDotnetClient 9.4.4 | Official IBM package, .NET 8 tested, managed mode |
| SignalR Client | @microsoft/signalr | Official Microsoft package for ASP.NET Core |
| MQ Docker Image | icr.io/ibm-messaging/mq:latest | Official IBM container, developer edition |
| Vue.js Integration | Service module pattern | Clean separation, lifecycle management |

---

## 1. IBM MQ .NET Client

### Decision: IBMMQDotnetClient 9.4.4

**Rationale**: Official IBM package, actively maintained, tested with .NET 8/9. Supports
managed client mode only (no unmanaged bindings needed for this use case).

**Alternatives Considered**:
- `IBMXMSDotnetClient` - JMS-style API, adds unnecessary abstraction
- `amqmdnet.dll` - .NET Framework only, not compatible with .NET 8

### Installation

```bash
dotnet add package IBMMQDotnetClient --version 9.4.4
```

### Connection Pattern

```csharp
using IBM.WMQ;
using System.Collections;

var properties = new Hashtable
{
    { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED },
    { MQC.HOST_NAME_PROPERTY, "localhost" },
    { MQC.PORT_PROPERTY, 1414 },
    { MQC.CHANNEL_PROPERTY, "DEV.APP.SVRCONN" },
    { MQC.USER_ID_PROPERTY, "app" },
    { MQC.PASSWORD_PROPERTY, "passw0rd" },
    { MQC.USE_MQCSP_AUTHENTICATION_PROPERTY, true }
};

using var queueManager = new MQQueueManager("QM1", properties);
```

### Queue Operations (Point-to-Point)

```csharp
// PUT message
var queue = queueManager.AccessQueue("DEV.QUEUE.1",
    MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
var message = new MQMessage();
message.WriteString("Hello!");
message.Format = MQC.MQFMT_STRING;
queue.Put(message, new MQPutMessageOptions());
queue.Close();

// GET message
var getQueue = queueManager.AccessQueue("DEV.QUEUE.1",
    MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);
var receiveMessage = new MQMessage();
var getOptions = new MQGetMessageOptions { WaitInterval = 5000 };
getOptions.Options = MQC.MQGMO_WAIT | MQC.MQGMO_FAIL_IF_QUIESCING;
getQueue.Get(receiveMessage, getOptions);
string content = receiveMessage.ReadString(receiveMessage.MessageLength);
```

### Topic Operations (Pub/Sub)

```csharp
// PUBLISH
var publisher = queueManager.AccessTopic(
    "dev/events",
    null,
    MQC.MQTOPIC_OPEN_AS_PUBLICATION,
    MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);
var pubMessage = new MQMessage();
pubMessage.WriteString("Event!");
publisher.Put(pubMessage, new MQPutMessageOptions());
publisher.Close();

// SUBSCRIBE
int subOptions = MQC.MQSO_CREATE | MQC.MQSO_FAIL_IF_QUIESCING |
                 MQC.MQSO_MANAGED | MQC.MQSO_NON_DURABLE;
var subscriber = queueManager.AccessTopic(
    "dev/events",
    null,
    MQC.MQTOPIC_OPEN_AS_SUBSCRIPTION,
    subOptions);
var subMessage = new MQMessage();
subscriber.Get(subMessage, new MQGetMessageOptions { WaitInterval = 5000 });
```

---

## 2. SignalR + Vue.js 2 Integration

### Decision: @microsoft/signalr with service module pattern

**Rationale**: Official Microsoft client, Vue.js 2 Options API compatible,
built-in automatic reconnection support.

**Alternatives Considered**:
- vue-signalr plugin - Outdated, not maintained
- Custom WebSocket - Reinventing the wheel, no auto-reconnect

### Installation

```bash
npm install @microsoft/signalr
```

### SignalR Service Module

```javascript
// src/services/signalrService.js
import * as signalR from '@microsoft/signalr';

let connection = null;

export function createConnection(hubUrl) {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

  return connection;
}

export function getConnection() {
  return connection;
}
```

### Vue Component Usage (Options API)

```javascript
export default {
  data() {
    return {
      connection: null,
      connectionState: 'Disconnected',
      messages: []
    };
  },

  async created() {
    this.connection = createConnection('/messageHub');

    // Register handlers BEFORE starting
    this.connection.on('MessageReceived', (msg) => {
      this.messages.push(msg);
    });

    this.connection.onreconnecting(() => {
      this.connectionState = 'Reconnecting';
    });

    this.connection.onreconnected(() => {
      this.connectionState = 'Connected';
    });

    this.connection.onclose(() => {
      this.connectionState = 'Disconnected';
    });

    await this.connection.start();
    this.connectionState = 'Connected';
  },

  beforeDestroy() {
    if (this.connection) {
      this.connection.stop();
    }
  }
};
```

---

## 3. IBM MQ Docker Setup

### Decision: icr.io/ibm-messaging/mq:latest

**Rationale**: Official IBM container with developer edition pre-configured.
Includes default queues, topics, and channels suitable for development.

### Default Developer Configuration

| Setting | Value |
|---------|-------|
| Queue Manager | QM1 |
| Listener Port | 1414 |
| Web Console | https://localhost:9443 |
| Channel | DEV.APP.SVRCONN |
| App User | app |
| Default Queues | DEV.QUEUE.1, DEV.QUEUE.2, DEV.QUEUE.3 |
| Default Topic | dev/ (via DEV.BASE.TOPIC) |

### Docker Compose Configuration

```yaml
version: '3.8'

services:
  ibmmq:
    image: icr.io/ibm-messaging/mq:latest
    container_name: ibmmq
    hostname: ibmmq
    environment:
      - LICENSE=accept
      - MQ_QMGR_NAME=QM1
    ports:
      - "1414:1414"   # MQ Listener
      - "9443:9443"   # Web Console
    volumes:
      - mqdata:/mnt/mqm
      - ./mq/config/20-playground.mqsc:/etc/mqm/20-playground.mqsc
    secrets:
      - mqAppPassword

volumes:
  mqdata:

secrets:
  mqAppPassword:
    file: ./secrets/app_password.txt
```

### Custom MQSC Configuration

```mqsc
* MQ Playground Pattern Queues
DEFINE QLOCAL('PLAYGROUND.P2P.QUEUE') DEFPSIST(NO) REPLACE
DEFINE QLOCAL('PLAYGROUND.REQUEST.QUEUE') DEFPSIST(NO) REPLACE
DEFINE QLOCAL('PLAYGROUND.REPLY.QUEUE') DEFPSIST(NO) REPLACE

* MQ Playground Topics
DEFINE TOPIC('PLAYGROUND.PUBSUB') TOPICSTR('playground/pubsub/') REPLACE

* Permissions for app user
SET AUTHREC PROFILE('PLAYGROUND.**') PRINCIPAL('app') OBJTYPE(QUEUE) AUTHADD(PUT,GET,BROWSE,INQ)
SET AUTHREC PROFILE('PLAYGROUND.**') PRINCIPAL('app') OBJTYPE(TOPIC) AUTHADD(PUB,SUB)
```

---

## 4. Request/Reply Pattern Implementation

### Decision: Use MQ Correlation ID with temporary reply queue

**Rationale**: Standard MQ pattern for synchronous request-response. Correlation ID
links responses to requests; temporary queue ensures isolation.

### Pattern

```csharp
// REQUEST
var requestQueue = queueManager.AccessQueue("PLAYGROUND.REQUEST.QUEUE",
    MQC.MQOO_OUTPUT);
var replyQueue = queueManager.AccessQueue("PLAYGROUND.REPLY.QUEUE",
    MQC.MQOO_INPUT_EXCLUSIVE);

var request = new MQMessage();
request.WriteString("Calculate: 2+2");
request.Format = MQC.MQFMT_STRING;
request.ReplyToQueueName = "PLAYGROUND.REPLY.QUEUE";
request.MessageType = MQC.MQMT_REQUEST;

var putOptions = new MQPutMessageOptions();
requestQueue.Put(request, putOptions);

// Store correlation ID for matching
byte[] correlationId = request.MessageId;

// WAIT FOR REPLY
var reply = new MQMessage();
reply.CorrelationId = correlationId;

var getOptions = new MQGetMessageOptions();
getOptions.Options = MQC.MQGMO_WAIT | MQC.MQGMO_MATCH_CORREL_ID;
getOptions.WaitInterval = 30000; // 30 second timeout

replyQueue.Get(reply, getOptions);
string response = reply.ReadString(reply.MessageLength);
```

---

## Key Implementation Notes

1. **Connection Management**: Use singleton MQ connection per application instance;
   SignalR handles per-client browser connections.

2. **Background Workers**: Use .NET Worker Services (IHostedService) for MQ consumers;
   they push to SignalR hub when messages arrive.

3. **Error Handling**: MQException provides reason codes; map to user-friendly messages
   in UI via SignalR.

4. **Tracing**: Set `MQDOTNET_TRACE_ON` environment variable for debugging; app.config
   not supported in .NET Standard.
