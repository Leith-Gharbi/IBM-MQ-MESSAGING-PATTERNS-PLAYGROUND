namespace MqPlayground.Api.Models;

/// <summary>
/// Configuration constants for MQ queue and topic names.
/// </summary>
public static class PatternConfig
{
    /// <summary>
    /// Point-to-Point pattern queue name.
    /// </summary>
    public const string P2PQueue = "PLAYGROUND.P2P.QUEUE";

    /// <summary>
    /// Request/Reply pattern - request queue name.
    /// </summary>
    public const string RequestQueue = "PLAYGROUND.REQUEST.QUEUE";

    /// <summary>
    /// Request/Reply pattern - reply queue name.
    /// </summary>
    public const string ReplyQueue = "PLAYGROUND.REPLY.QUEUE";

    /// <summary>
    /// Pub/Sub pattern - topic name.
    /// </summary>
    public const string PubSubTopic = "PLAYGROUND.PUBSUB";

    /// <summary>
    /// Pub/Sub pattern - topic string for subscriptions.
    /// </summary>
    public const string PubSubTopicString = "playground/pubsub/";

    /// <summary>
    /// Maximum messages per panel (FIFO eviction when exceeded).
    /// </summary>
    public const int MaxMessagesPerPanel = 50;

    /// <summary>
    /// Default timeout for Request/Reply pattern (seconds).
    /// </summary>
    public const int DefaultRequestTimeoutSeconds = 30;

    /// <summary>
    /// Minimum number of subscribers for Pub/Sub pattern.
    /// </summary>
    public const int MinSubscribers = 2;

    /// <summary>
    /// Maximum number of subscribers for Pub/Sub pattern.
    /// </summary>
    public const int MaxSubscribers = 4;
}
