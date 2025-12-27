namespace MqPlayground.Api.Models;

/// <summary>
/// Represents a message sent or received through IBM MQ.
/// </summary>
public class Message
{
    /// <summary>
    /// Unique identifier for the message (GUID).
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Message text content (max 10,000 characters).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When the message was sent or received (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the message was sent or received.
    /// </summary>
    public MessageDirection Direction { get; set; }

    /// <summary>
    /// Links request to reply (optional, for Request/Reply pattern).
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Which messaging pattern this message belongs to.
    /// </summary>
    public MessagePattern Pattern { get; set; }
}

/// <summary>
/// Direction of the message flow.
/// </summary>
public enum MessageDirection
{
    Sent,
    Received
}

/// <summary>
/// MQ messaging pattern types.
/// </summary>
public enum MessagePattern
{
    PointToPoint,
    PublishSubscribe,
    RequestReply
}
