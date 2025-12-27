using Microsoft.AspNetCore.SignalR;
using MqPlayground.Api.Models;
using MqPlayground.Api.Services;

namespace MqPlayground.Api.Hubs;

/// <summary>
/// SignalR hub for real-time message notifications.
///
/// WHY: This hub bridges MQ events to browser clients. When a message
/// is received from MQ (by background workers), it's pushed to all
/// connected clients via SignalR, enabling real-time visualization
/// of messaging patterns.
///
/// Server-to-Client Events:
/// - MessageReceived: When a message arrives from MQ (P2P, Pub/Sub, Request/Reply)
/// - SubscriberMessageReceived: When a specific Pub/Sub subscriber receives a message
/// - RequestStatusChanged: When a request receives a reply or times out
/// - ConnectionStatusChanged: When MQ connection state changes
/// - SubscriberAdded: When a new Pub/Sub subscriber is created
/// - SubscriberRemoved: When a Pub/Sub subscriber is deleted
///
/// Client-to-Server Methods:
/// - JoinSession: Register for updates, receive initial state
/// - LeaveSession: Unregister before disconnect
/// </summary>
public class MessageHub : Hub
{
    private readonly ILogger<MessageHub> _logger;
    private readonly IMqConnectionService _mqConnection;

    public MessageHub(
        ILogger<MessageHub> logger,
        IMqConnectionService mqConnection)
    {
        _logger = logger;
        _mqConnection = mqConnection;
    }

    /// <summary>
    /// Registers client for real-time updates. Called on connection start.
    /// </summary>
    /// <returns>SessionInfo with current connection status and subscribers.</returns>
    public async Task<SessionInfo> JoinSession()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Client {ConnectionId} joined session", connectionId);

        await Groups.AddToGroupAsync(connectionId, "Playground");

        return new SessionInfo
        {
            ConnectionStatus = _mqConnection.GetStatus(),
            Subscribers = new List<Subscriber>() // Will be populated by PubSubService
        };
    }

    /// <summary>
    /// Unregisters client from updates. Called on connection close.
    /// </summary>
    public async Task LeaveSession()
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("Client {ConnectionId} left session", connectionId);

        await Groups.RemoveFromGroupAsync(connectionId, "Playground");
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Initial session state returned when client joins.
/// </summary>
public class SessionInfo
{
    public ConnectionStatus ConnectionStatus { get; set; } = new();
    public List<Subscriber> Subscribers { get; set; } = new();
}
