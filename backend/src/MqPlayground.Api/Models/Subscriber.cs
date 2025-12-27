namespace MqPlayground.Api.Models;

/// <summary>
/// Represents a topic subscriber in the Pub/Sub pattern panel.
/// </summary>
public class Subscriber
{
    /// <summary>
    /// Unique subscriber ID (GUID).
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name (e.g., "Subscriber 1").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Currently subscribed to topic.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Number of messages received by this subscriber.
    /// </summary>
    public int MessageCount { get; set; }
}
