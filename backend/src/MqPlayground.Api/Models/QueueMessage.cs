namespace MqPlayground.Api.Models;

/// <summary>
/// Represents a message currently in an MQ queue for visualization purposes.
/// </summary>
public class QueueMessage
{
    /// <summary>
    /// Unique identifier for tracking (GUID).
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Message text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the message was added to the queue (UTC).
    /// </summary>
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Name of the queue containing this message.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Which messaging pattern this message belongs to.
    /// </summary>
    public MessagePattern Pattern { get; set; }

    /// <summary>
    /// Correlation ID for Request/Reply pattern (optional).
    /// </summary>
    public string? CorrelationId { get; set; }
}
