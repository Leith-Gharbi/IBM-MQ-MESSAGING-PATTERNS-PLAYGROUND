namespace MqPlayground.Api.Models;

/// <summary>
/// Represents the current connection state to IBM MQ.
/// </summary>
public class ConnectionStatus
{
    /// <summary>
    /// Whether MQ is reachable.
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Name of the connected Queue Manager.
    /// </summary>
    public string? QueueManagerName { get; set; }

    /// <summary>
    /// When status was last verified (UTC).
    /// </summary>
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Description if disconnected.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
