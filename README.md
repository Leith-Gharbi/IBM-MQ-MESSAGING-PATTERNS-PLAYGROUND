# MQ Patterns Playground

An interactive web application demonstrating three IBM MQ messaging patterns with real-time visualization.

## Messaging Patterns

### Point-to-Point
One producer sends messages to a queue, one consumer receives them. Messages are processed exactly once.

**Try it:** Type "Hello MQ" in the input and click Send. Watch the message appear in the consumer section.

### Publish/Subscribe
One publisher sends to a topic, multiple subscribers receive copies of each message simultaneously.

**Try it:** With 2+ subscribers active, publish "Breaking News!" and watch all subscribers receive it.

### Request/Reply
Synchronous-style communication with correlation IDs linking requests to their responses.

**Try it:** Send "Calculate: 2+2" and watch the request flow through, receiving response "4".

## Quick Start

### Prerequisites
- Docker Desktop (with Docker Compose)
- .NET 8 SDK
- Node.js 18+

### 1. Start IBM MQ
```bash
cd docker
docker compose up -d ibmmq
```

Wait for MQ to initialize (check logs for `AMQ5026I: The listener 'DEV.LISTENER.TCP' has started.`)

### 2. Start the Backend
```bash
cd backend/src/MqPlayground.Api
dotnet run
```
API runs on `http://localhost:5000`

### 3. Start the Frontend
```bash
cd frontend
npm install
npm run serve
```
UI available at `http://localhost:8080`

### 4. Open the Playground
Navigate to `http://localhost:8080` - you'll see a 3-column layout with all three patterns.

## Project Structure

```
backend/
├── src/MqPlayground.Api/
│   ├── Controllers/     # REST API endpoints
│   ├── Hubs/           # SignalR hub for real-time updates
│   ├── Models/         # Data entities
│   ├── Services/       # MQ pattern implementations
│   └── Workers/        # Background message consumers
frontend/
├── src/
│   ├── components/     # Reusable Vue components
│   ├── panels/        # Pattern-specific panels
│   └── services/      # SignalR client
docker/
├── docker-compose.yml
└── mq/config/         # MQ queue/topic definitions
```

## MQ Queue Names

| Pattern | Queue/Topic |
|---------|-------------|
| Point-to-Point | PLAYGROUND.P2P.QUEUE |
| Pub/Sub | PLAYGROUND.PUBSUB (topic) |
| Request/Reply | PLAYGROUND.REQUEST.QUEUE, PLAYGROUND.REPLY.QUEUE |

## Technology Stack

- **Backend:** .NET 8, ASP.NET Core Web API, SignalR
- **Frontend:** Vue.js 2, @microsoft/signalr
- **Message Broker:** IBM MQ (Docker)
- **MQ Client:** IBMMQDotnetClient 9.4.4

## MQ Web Console

Access at `https://localhost:9443/ibmmq/console`
- Username: `admin`
- Password: (from docker/secrets/app_password.txt)

## Troubleshooting

### MQ Container Issues
```bash
docker compose logs ibmmq
```

### Backend Connection Issues
1. Verify MQ is running: `docker compose ps`
2. Check port 1414: `netstat -an | grep 1414`
3. Verify appsettings.json credentials match MQ config

### Frontend Connection Issues
1. Check browser console for WebSocket errors
2. Verify backend is running on port 5000
3. Check CORS configuration

## Stop Everything
```bash
docker compose down
# To also remove data volumes:
docker compose down -v
```

## Documentation

- [Specification](specs/001-mq-patterns-playground/spec.md)
- [Implementation Plan](specs/001-mq-patterns-playground/plan.md)
- [Quickstart Guide](specs/001-mq-patterns-playground/quickstart.md)
