using MqPlayground.Api.Models;

namespace MqPlayground.Api.Services;

/// <summary>
/// Service interface for tracking and broadcasting queue state changes.
/// </summary>
public interface IQueueBrowserService
{
    /// <summary>
    /// Gets the configured delay before message consumption (in milliseconds).
    /// </summary>
    int ConsumptionDelayMs { get; }

    /// <summary>
    /// Notifies that a message has been added to a queue.
    /// Broadcasts to connected clients via SignalR.
    /// </summary>
    Task NotifyMessageEnqueued(QueueMessage message);

    /// <summary>
    /// Notifies that a message has been consumed from a queue.
    /// Broadcasts to connected clients via SignalR.
    /// </summary>
    Task NotifyMessageDequeued(string messageId, string queueName);

    /// <summary>
    /// Gets all messages currently tracked in a specific queue.
    /// </summary>
    IEnumerable<QueueMessage> GetQueueMessages(string queueName);

    /// <summary>
    /// Clears all tracked messages for a specific queue.
    /// </summary>
    Task ClearQueue(string queueName);
}
