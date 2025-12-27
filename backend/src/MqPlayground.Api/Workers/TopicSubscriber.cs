using IBM.WMQ;
using MqPlayground.Api.Models;
using MqPlayground.Api.Services;

namespace MqPlayground.Api.Workers;

/// <summary>
/// Background worker that manages topic subscriptions for Pub/Sub pattern.
///
/// WHY: This worker creates and manages MQ topic subscriptions for each
/// active subscriber. When messages are published to the topic, this
/// worker receives them and distributes to all subscribed clients via
/// the PubSubService, demonstrating one-to-many message distribution.
/// </summary>
public class TopicSubscriber : BackgroundService
{
    private readonly ILogger<TopicSubscriber> _logger;
    private readonly IMqConnectionService _mqConnection;
    private readonly PubSubService _pubSubService;

    public TopicSubscriber(
        ILogger<TopicSubscriber> logger,
        IMqConnectionService mqConnection,
        PubSubService pubSubService)
    {
        _logger = logger;
        _mqConnection = mqConnection;
        _pubSubService = pubSubService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Topic subscriber starting...");

        // Wait for MQ connection
        await Task.Delay(3000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SubscribeToTopic(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in topic subscriber. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("Topic subscriber stopped.");
    }

    private async Task SubscribeToTopic(CancellationToken stoppingToken)
    {
        var queueManager = _mqConnection.GetQueueManager() as MQQueueManager;
        if (queueManager == null || !_mqConnection.IsConnected)
        {
            _logger.LogWarning("Not connected to MQ. Waiting...");
            await Task.Delay(5000, stoppingToken);
            return;
        }

        MQTopic? subscription = null;
        try
        {
            // Create a managed subscription to the topic
            int subOptions = MQC.MQSO_CREATE | MQC.MQSO_FAIL_IF_QUIESCING |
                            MQC.MQSO_MANAGED | MQC.MQSO_NON_DURABLE;

            subscription = queueManager.AccessTopic(
                PatternConfig.PubSubTopicString,
                null,
                MQC.MQTOPIC_OPEN_AS_SUBSCRIPTION,
                subOptions);

            _logger.LogInformation("Subscribed to topic: {Topic}", PatternConfig.PubSubTopicString);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var mqMessage = new MQMessage();
                    var getOptions = new MQGetMessageOptions
                    {
                        WaitInterval = 5000,
                        Options = MQC.MQGMO_WAIT | MQC.MQGMO_FAIL_IF_QUIESCING
                    };

                    subscription.Get(mqMessage, getOptions);

                    var content = mqMessage.ReadString(mqMessage.MessageLength);

                    _logger.LogInformation("Message received from topic: {Content}", content);

                    // Deliver message to all active subscribers
                    var subscribers = _pubSubService.GetSubscribers()
                        .Where(s => s.IsActive)
                        .ToList();

                    foreach (var subscriber in subscribers)
                    {
                        await _pubSubService.HandleMessageReceived(subscriber.Id, content);
                    }
                }
                catch (MQException ex) when (ex.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
                {
                    // No message available, continue polling
                    continue;
                }
            }
        }
        finally
        {
            subscription?.Close();
        }
    }
}
