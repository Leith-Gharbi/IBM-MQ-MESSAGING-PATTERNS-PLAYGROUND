using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using MqPlayground.Api.Hubs;
using MqPlayground.Api.Models;

namespace MqPlayground.Api.Services;

/// <summary>
/// Tracks queue state and broadcasts changes via SignalR for real-time visualization.
///
/// WHY: This service enables users to see messages flowing through queues.
/// It maintains an in-memory representation of queue contents and notifies
/// connected clients when messages are enqueued or dequeued.
/// </summary>
public class QueueBrowserService : IQueueBrowserService
{
    private readonly IHubContext<MessageHub> _hubContext;
    private readonly ILogger<QueueBrowserService> _logger;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, QueueMessage>> _queues = new();

    /// <summary>
    /// Delay in milliseconds before consuming a message (allows visualization).
    /// </summary>
    public int ConsumptionDelayMs { get; } = 2000;

    public QueueBrowserService(
        IHubContext<MessageHub> hubContext,
        ILogger<QueueBrowserService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyMessageEnqueued(QueueMessage message)
    {
        // Add to internal tracking
        var queue = _queues.GetOrAdd(message.QueueName, _ => new ConcurrentDictionary<string, QueueMessage>());
        queue.TryAdd(message.Id, message);

        _logger.LogDebug(
            "Message {MessageId} enqueued to {QueueName}. Queue depth: {Depth}",
            message.Id, message.QueueName, queue.Count);

        // Broadcast to connected clients
        await _hubContext.Clients.Group("Playground")
            .SendAsync("QueueMessageAdded", message);
    }

    public async Task NotifyMessageDequeued(string messageId, string queueName)
    {
        // Remove from internal tracking
        if (_queues.TryGetValue(queueName, out var queue))
        {
            queue.TryRemove(messageId, out _);

            _logger.LogDebug(
                "Message {MessageId} dequeued from {QueueName}. Queue depth: {Depth}",
                messageId, queueName, queue.Count);
        }

        // Broadcast to connected clients
        await _hubContext.Clients.Group("Playground")
            .SendAsync("QueueMessageRemoved", new { messageId, queueName });
    }

    public IEnumerable<QueueMessage> GetQueueMessages(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue))
        {
            return queue.Values.OrderBy(m => m.EnqueuedAt);
        }
        return Enumerable.Empty<QueueMessage>();
    }

    public async Task ClearQueue(string queueName)
    {
        if (_queues.TryGetValue(queueName, out var queue))
        {
            var messageIds = queue.Keys.ToList();
            queue.Clear();

            // Notify clients about all removed messages
            foreach (var messageId in messageIds)
            {
                await _hubContext.Clients.Group("Playground")
                    .SendAsync("QueueMessageRemoved", new { messageId, queueName });
            }

            _logger.LogInformation("Queue {QueueName} cleared. Removed {Count} messages", queueName, messageIds.Count);
        }
    }
}
