using System.Collections.Concurrent;
using IBM.WMQ;
using Microsoft.AspNetCore.SignalR;
using MqPlayground.Api.Hubs;
using MqPlayground.Api.Models;

namespace MqPlayground.Api.Services;

/// <summary>
/// Service for Publish/Subscribe messaging pattern.
///
/// WHY: Pub/Sub enables one-to-many message delivery. A publisher sends
/// messages to a topic, and all active subscribers receive copies of
/// those messages. This demonstrates the MQ topic/subscription model
/// where messages are distributed to multiple consumers simultaneously.
/// </summary>
public class PubSubService
{
    private readonly ILogger<PubSubService> _logger;
    private readonly IMqConnectionService _mqConnection;
    private readonly IHubContext<MessageHub> _hubContext;
    private readonly IQueueBrowserService _queueBrowser;
    private readonly ConcurrentDictionary<string, Subscriber> _subscribers = new();
    private int _subscriberCounter;

    public PubSubService(
        ILogger<PubSubService> logger,
        IMqConnectionService mqConnection,
        IHubContext<MessageHub> hubContext,
        IQueueBrowserService queueBrowser)
    {
        _logger = logger;
        _mqConnection = mqConnection;
        _hubContext = hubContext;
        _queueBrowser = queueBrowser;

        // Initialize with minimum 2 subscribers
        AddSubscriber();
        AddSubscriber();
    }

    /// <summary>
    /// Gets all current subscribers.
    /// </summary>
    public IEnumerable<Subscriber> GetSubscribers()
    {
        return _subscribers.Values.ToList();
    }

    /// <summary>
    /// Adds a new subscriber if under maximum limit.
    /// </summary>
    /// <returns>The new subscriber, or null if max reached.</returns>
    public Subscriber? AddSubscriber()
    {
        if (_subscribers.Count >= PatternConfig.MaxSubscribers)
        {
            _logger.LogWarning("Cannot add subscriber: maximum ({Max}) reached", PatternConfig.MaxSubscribers);
            return null;
        }

        var subscriberNumber = Interlocked.Increment(ref _subscriberCounter);
        var subscriber = new Subscriber
        {
            Name = $"Subscriber {subscriberNumber}",
            IsActive = true,
            MessageCount = 0
        };

        _subscribers.TryAdd(subscriber.Id, subscriber);
        _logger.LogInformation("Added subscriber: {Name} ({Id})", subscriber.Name, subscriber.Id);

        // Notify all clients
        _hubContext.Clients.Group("Playground")
            .SendAsync("SubscriberAdded", subscriber);

        return subscriber;
    }

    /// <summary>
    /// Removes a subscriber if above minimum limit.
    /// </summary>
    /// <param name="subscriberId">The subscriber ID to remove.</param>
    /// <returns>True if removed, false if not found or below minimum.</returns>
    public bool RemoveSubscriber(string subscriberId)
    {
        if (_subscribers.Count <= PatternConfig.MinSubscribers)
        {
            _logger.LogWarning("Cannot remove subscriber: minimum ({Min}) required", PatternConfig.MinSubscribers);
            return false;
        }

        if (_subscribers.TryRemove(subscriberId, out var removed))
        {
            _logger.LogInformation("Removed subscriber: {Name} ({Id})", removed.Name, removed.Id);

            // Notify all clients
            _hubContext.Clients.Group("Playground")
                .SendAsync("SubscriberRemoved", subscriberId);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Publishes a message to the Pub/Sub topic.
    /// </summary>
    /// <param name="content">The message content to publish.</param>
    /// <returns>The published message.</returns>
    public async Task<Message> PublishMessageAsync(string content)
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
            Pattern = MessagePattern.PublishSubscribe,
            Timestamp = DateTime.UtcNow
        };

        MQTopic? topic = null;
        try
        {
            // Open topic for publishing
            topic = queueManager.AccessTopic(
                PatternConfig.PubSubTopicString,
                null,
                MQC.MQTOPIC_OPEN_AS_PUBLICATION,
                MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);

            // Create MQ message
            var mqMessage = new MQMessage();
            mqMessage.WriteString(content);
            mqMessage.Format = MQC.MQFMT_STRING;

            // Publish message to topic
            topic.Put(mqMessage, new MQPutMessageOptions());

            _logger.LogInformation(
                "Message published to topic {Topic}: {MessageId}",
                PatternConfig.PubSubTopicString,
                message.Id);

            // Notify queue browser for visualization (shared topic queue)
            var queueMessage = new QueueMessage
            {
                Id = message.Id,
                Content = content,
                QueueName = PatternConfig.PubSubTopicString,
                Pattern = MessagePattern.PublishSubscribe,
                EnqueuedAt = message.Timestamp
            };
            await _queueBrowser.NotifyMessageEnqueued(queueMessage);

            return message;
        }
        catch (MQException ex)
        {
            _logger.LogError(ex,
                "Failed to publish message to topic {Topic}. Reason code: {ReasonCode}",
                PatternConfig.PubSubTopicString,
                ex.ReasonCode);
            throw;
        }
        finally
        {
            topic?.Close();
        }
    }

    /// <summary>
    /// Handles a received message for a specific subscriber.
    /// Called by TopicSubscriber worker.
    /// </summary>
    public async Task HandleMessageReceived(string subscriberId, string content)
    {
        if (!_subscribers.TryGetValue(subscriberId, out var subscriber))
        {
            return;
        }

        subscriber.MessageCount++;

        var message = new Message
        {
            Content = content,
            Direction = MessageDirection.Received,
            Pattern = MessagePattern.PublishSubscribe,
            Timestamp = DateTime.UtcNow
        };

        // Notify clients about subscriber-specific message
        await _hubContext.Clients.Group("Playground")
            .SendAsync("SubscriberMessageReceived", subscriberId, message);
    }
}
