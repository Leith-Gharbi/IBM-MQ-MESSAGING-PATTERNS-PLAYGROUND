namespace MqPlayground.Api.Models;

/// <summary>
/// Tracks pending requests awaiting replies in the Request/Reply pattern.
/// </summary>
public class Request
{
    /// <summary>
    /// Request message ID (GUID).
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Correlation ID for matching request to reply.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Request message content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// When request was sent (UTC).
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Current request status.
    /// </summary>
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    /// <summary>
    /// The reply message if received.
    /// </summary>
    public Message? Response { get; set; }

    /// <summary>
    /// Timeout duration in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = PatternConfig.DefaultRequestTimeoutSeconds;
}

/// <summary>
/// Request status values.
/// </summary>
public enum RequestStatus
{
    Pending,
    Completed,
    TimedOut
}
