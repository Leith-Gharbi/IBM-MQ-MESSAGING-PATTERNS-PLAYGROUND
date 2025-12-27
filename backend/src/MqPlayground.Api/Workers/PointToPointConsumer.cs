using IBM.WMQ;
using Microsoft.AspNetCore.SignalR;
using MqPlayground.Api.Hubs;
using MqPlayground.Api.Models;
using MqPlayground.Api.Services;

namespace MqPlayground.Api.Workers;

/// <summary>
/// Background worker that consumes messages from the Point-to-Point queue.
///
/// WHY: This worker runs continuously in the background, waiting for messages
/// on the P2P queue. When a message arrives, it reads the content and pushes
/// it to all connected browser clients via SignalR. This enables real-time
/// visualization of the Point-to-Point pattern.
/// </summary>
public class PointToPointConsumer : BackgroundService
{
    private readonly ILogger<PointToPointConsumer> _logger;
    private readonly IMqConnectionService _mqConnection;
    private readonly IHubContext<MessageHub> _hubContext;

    public PointToPointConsumer(
        ILogger<PointToPointConsumer> logger,
        IMqConnectionService mqConnection,
        IHubContext<MessageHub> hubContext)
    {
        _logger = logger;
        _mqConnection = mqConnection;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Point-to-Point consumer starting...");

        // Wait a bit for MQ connection to be established
        await Task.Delay(2000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeMessages(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Point-to-Point consumer. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Point-to-Point consumer stopped.");
    }

    private async Task ConsumeMessages(CancellationToken stoppingToken)
    {
        var queueManager = _mqConnection.GetQueueManager() as MQQueueManager;
        if (queueManager == null || !_mqConnection.IsConnected)
        {
            _logger.LogWarning("Not connected to MQ. Waiting...");
            await Task.Delay(5000, stoppingToken);
            return;
        }

        MQQueue? queue = null;
        try
        {
            // Open queue for input (getting messages)
            queue = queueManager.AccessQueue(
                PatternConfig.P2PQueue,
                MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);

            _logger.LogInformation("Listening on queue: {Queue}", PatternConfig.P2PQueue);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var mqMessage = new MQMessage();
                    var getOptions = new MQGetMessageOptions
                    {
                        WaitInterval = 5000, // Wait 5 seconds for message
                        Options = MQC.MQGMO_WAIT | MQC.MQGMO_FAIL_IF_QUIESCING
                    };

                    queue.Get(mqMessage, getOptions);

                    // Read message content
                    var content = mqMessage.ReadString(mqMessage.MessageLength);

                    var message = new Message
                    {
                        Content = content,
                        Direction = MessageDirection.Received,
                        Pattern = MessagePattern.PointToPoint,
                        Timestamp = DateTime.UtcNow
                    };

                    _logger.LogInformation(
                        "Message received from {Queue}: {MessageId}",
                        PatternConfig.P2PQueue,
                        message.Id);

                    // Broadcast to all connected clients via SignalR
                    await _hubContext.Clients.Group("Playground")
                        .SendAsync("MessageReceived", message, stoppingToken);
                }
                catch (MQException ex) when (ex.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
                {
                    // No message available within wait interval, continue polling
                    continue;
                }
            }
        }
        finally
        {
            queue?.Close();
        }
    }
}
