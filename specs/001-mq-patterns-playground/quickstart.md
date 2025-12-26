# Quickstart: MQ Patterns Playground

Get the MQ Patterns Playground running locally in under 5 minutes.

## Prerequisites

- Docker Desktop (with Docker Compose)
- .NET 8 SDK
- Node.js 18+ and npm

## Quick Start

### 1. Clone and Navigate

```bash
cd IBM_MQ_PROJECT
```

### 2. Start IBM MQ

```bash
docker compose up -d ibmmq
```

Wait for MQ to initialize (check with `docker compose logs ibmmq`). Look for:
```
AMQ5026I: The listener 'DEV.LISTENER.TCP' has started.
```

### 3. Start the Backend

```bash
cd backend/src/MqPlayground.Api
dotnet run
```

The API will start on `http://localhost:5000`.

### 4. Start the Frontend

In a new terminal:

```bash
cd frontend
npm install
npm run serve
```

The UI will be available at `http://localhost:8080`.

### 5. Open the Playground

Navigate to `http://localhost:8080` in your browser.

---

## What You'll See

A 3-column layout with panels for each messaging pattern:

```text
┌─────────────────┬─────────────────┬─────────────────┐
│  Point-to-Point │  Pub/Sub        │  Request/Reply  │
├─────────────────┼─────────────────┼─────────────────┤
│  [Message Input]│  [Message Input]│  [Message Input]│
│  [Send Button]  │  [Publish]      │  [Send Request] │
├─────────────────┼─────────────────┼─────────────────┤
│  Consumer:      │  Subscriber 1:  │  Responses:     │
│  - Message 1    │  - Message 1    │  - Request 1 →  │
│  - Message 2    │  Subscriber 2:  │    Response 1   │
│                 │  - Message 1    │                 │
│                 │  [+] [-]        │                 │
└─────────────────┴─────────────────┴─────────────────┘
```

---

## Try Each Pattern

### Point-to-Point

1. Type a message in the input field
2. Click "Send"
3. Watch the message appear in the Consumer section

### Publish/Subscribe

1. Type a message in the input field
2. Click "Publish"
3. Watch all subscribers receive the same message
4. Click [+] to add a subscriber (up to 4)
5. Click [-] to remove a subscriber (minimum 2)

### Request/Reply

1. Type a request (e.g., "2+2")
2. Click "Send Request"
3. Watch the request flow to the responder
4. See the correlated response appear with matching ID

---

## Troubleshooting

### MQ Container Won't Start

```bash
docker compose logs ibmmq
```

Check for license acceptance issues or port conflicts.

### Backend Can't Connect to MQ

1. Verify MQ is running: `docker compose ps`
2. Check port 1414 is exposed: `netstat -an | grep 1414`
3. Verify credentials in appsettings.json match MQ config

### Frontend Can't Connect to Backend

1. Check CORS is configured in backend
2. Verify backend is running on port 5000
3. Check browser console for connection errors

### SignalR Connection Drops

The client has automatic reconnection. If issues persist:
1. Check browser console for WebSocket errors
2. Verify SignalR hub is mapped in Program.cs
3. Check firewall isn't blocking WebSocket connections

---

## MQ Web Console

Access the IBM MQ web console at: `https://localhost:9443/ibmmq/console`

- Username: `admin`
- Password: (from secrets/admin_password.txt)

Use this to:
- View queue depths
- Browse messages
- Monitor connections

---

## Stop Everything

```bash
docker compose down
```

To also remove MQ data volumes:

```bash
docker compose down -v
```

---

## Next Steps

- Read the [spec.md](./spec.md) for full feature requirements
- Check [research.md](./research.md) for technology decisions
- Review [data-model.md](./data-model.md) for entity definitions
- See [contracts/](./contracts/) for API specifications
