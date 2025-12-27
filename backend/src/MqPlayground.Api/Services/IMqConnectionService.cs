using MqPlayground.Api.Models;

namespace MqPlayground.Api.Services;

/// <summary>
/// Interface for IBM MQ connection management.
/// Provides methods to connect, disconnect, and check connection status.
/// </summary>
public interface IMqConnectionService
{
    /// <summary>
    /// Establishes a connection to the IBM MQ Queue Manager.
    /// </summary>
    /// <returns>True if connection successful, false otherwise.</returns>
    Task<bool> ConnectAsync();

    /// <summary>
    /// Disconnects from the IBM MQ Queue Manager.
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Gets the current connection status.
    /// </summary>
    /// <returns>ConnectionStatus with current state and details.</returns>
    ConnectionStatus GetStatus();

    /// <summary>
    /// Checks if currently connected to the Queue Manager.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the underlying MQ Queue Manager for direct operations.
    /// Returns null if not connected.
    /// </summary>
    object? GetQueueManager();
}
