using IBM.WMQ;
using MqPlayground.Api.Models;

namespace MqPlayground.Api.Services;

/// <summary>
/// Service for Point-to-Point messaging pattern.
///
/// WHY: Point-to-Point is the simplest MQ pattern - one producer sends
/// messages to a queue, and one consumer receives them. Messages are
/// removed from the queue once consumed, ensuring each message is
/// processed exactly once.
/// </summary>
public class PointToPointService
{
    private readonly ILogger<PointToPointService> _logger;
    private readonly IMqConnectionService _mqConnection;

    public PointToPointService(
        ILogger<PointToPointService> logger,
        IMqConnectionService mqConnection)
    {
        _logger = logger;
        _mqConnection = mqConnection;
    }

    /// <summary>
    /// Sends a message to the Point-to-Point queue.
    /// </summary>
    /// <param name="content">The message content to send.</param>
    /// <returns>The sent message with generated ID and timestamp.</returns>
    public Message SendMessage(string content)
    {
        var queueManager = _mqConnection.GetQueueManager() as MQQueueManager;
        if (queueManager == null || !_mqConnection.IsConnected)
        {
            throw new InvalidOperationException("Not connected to MQ");
        }

        var message = new Message
        {
            Content = content,
            Direction = MessageDirection.Sent,
            Pattern = MessagePattern.PointToPoint,
            Timestamp = DateTime.UtcNow
        };

        MQQueue? queue = null;
        try
        {
            // Open queue for output (putting messages)
            queue = queueManager.AccessQueue(
                PatternConfig.P2PQueue,
                MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);

            // Create MQ message
            var mqMessage = new MQMessage();
            mqMessage.WriteString(content);
            mqMessage.Format = MQC.MQFMT_STRING;

            // Put message to queue
            var putOptions = new MQPutMessageOptions();
            queue.Put(mqMessage, putOptions);

            _logger.LogInformation(
                "Message sent to {Queue}: {MessageId}",
                PatternConfig.P2PQueue,
                message.Id);

            return message;
        }
        catch (MQException ex)
        {
            _logger.LogError(ex,
                "Failed to send message to {Queue}. Reason code: {ReasonCode}",
                PatternConfig.P2PQueue,
                ex.ReasonCode);
            throw;
        }
        finally
        {
            queue?.Close();
        }
    }
}
