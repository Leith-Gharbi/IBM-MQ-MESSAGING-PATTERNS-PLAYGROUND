using System.Collections;
using IBM.WMQ;
using MqPlayground.Api.Models;

namespace MqPlayground.Api.Services;

/// <summary>
/// Manages IBM MQ connection lifecycle with retry logic.
///
/// WHY: This service centralizes MQ connection management to ensure
/// all pattern implementations share a single connection to the
/// Queue Manager, following MQ best practices for connection pooling.
/// </summary>
public class MqConnectionService : IMqConnectionService, IDisposable
{
    private readonly ILogger<MqConnectionService> _logger;
    private readonly IConfiguration _configuration;
    private MQQueueManager? _queueManager;
    private readonly object _lock = new();
    private bool _disposed;

    public MqConnectionService(
        ILogger<MqConnectionService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public bool IsConnected => _queueManager != null && _queueManager.IsConnected;

    public async Task<bool> ConnectAsync()
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                if (IsConnected)
                {
                    _logger.LogDebug("Already connected to MQ");
                    return true;
                }

                try
                {
                    var properties = CreateConnectionProperties();
                    var queueManagerName = _configuration["MQ:QueueManager"] ?? "QM1";

                    _logger.LogInformation("Connecting to MQ Queue Manager: {QueueManager}", queueManagerName);

                    _queueManager = new MQQueueManager(queueManagerName, properties);

                    _logger.LogInformation("Successfully connected to MQ Queue Manager: {QueueManager}", queueManagerName);
                    return true;
                }
                catch (MQException ex)
                {
                    _logger.LogError(ex, "Failed to connect to MQ. Reason code: {ReasonCode}", ex.ReasonCode);
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error connecting to MQ");
                    return false;
                }
            }
        });
    }

    public async Task DisconnectAsync()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_queueManager != null)
                {
                    try
                    {
                        _queueManager.Disconnect();
                        _logger.LogInformation("Disconnected from MQ Queue Manager");
                    }
                    catch (MQException ex)
                    {
                        _logger.LogWarning(ex, "Error disconnecting from MQ. Reason code: {ReasonCode}", ex.ReasonCode);
                    }
                    finally
                    {
                        _queueManager = null;
                    }
                }
            }
        });
    }

    public ConnectionStatus GetStatus()
    {
        lock (_lock)
        {
            if (_queueManager != null && _queueManager.IsConnected)
            {
                return new ConnectionStatus
                {
                    IsConnected = true,
                    QueueManagerName = _queueManager.Name,
                    LastChecked = DateTime.UtcNow
                };
            }

            return new ConnectionStatus
            {
                IsConnected = false,
                LastChecked = DateTime.UtcNow,
                ErrorMessage = "Not connected to Queue Manager"
            };
        }
    }

    public object? GetQueueManager()
    {
        lock (_lock)
        {
            return _queueManager;
        }
    }

    private Hashtable CreateConnectionProperties()
    {
        var host = _configuration["MQ:Host"] ?? "localhost";
        var port = int.Parse(_configuration["MQ:Port"] ?? "1414");
        var channel = _configuration["MQ:Channel"] ?? "DEV.APP.SVRCONN";
        var user = _configuration["MQ:User"] ?? "app";

        // Read password from file or configuration
        var password = ReadPassword();

        return new Hashtable
        {
            { MQC.TRANSPORT_PROPERTY, MQC.TRANSPORT_MQSERIES_MANAGED },
            { MQC.HOST_NAME_PROPERTY, host },
            { MQC.PORT_PROPERTY, port },
            { MQC.CHANNEL_PROPERTY, channel },
            { MQC.USER_ID_PROPERTY, user },
            { MQC.PASSWORD_PROPERTY, password },
            { MQC.USE_MQCSP_AUTHENTICATION_PROPERTY, true }
        };
    }

    private string ReadPassword()
    {
        var passwordFile = _configuration["MQ:PasswordFile"];
        if (!string.IsNullOrEmpty(passwordFile) && File.Exists(passwordFile))
        {
            return File.ReadAllText(passwordFile).Trim();
        }

        return _configuration["MQ:Password"] ?? "passw0rd";
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            if (_queueManager != null)
            {
                try
                {
                    _queueManager.Disconnect();
                }
                catch
                {
                    // Ignore errors during disposal
                }
                _queueManager = null;
            }
        }

        _disposed = true;
    }
}
