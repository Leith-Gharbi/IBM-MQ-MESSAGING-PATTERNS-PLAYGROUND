using System.Text.RegularExpressions;
using IBM.WMQ;
using MqPlayground.Api.Models;
using MqPlayground.Api.Services;
using MessagePattern = MqPlayground.Api.Models.MessagePattern;

namespace MqPlayground.Api.Workers;

/// <summary>
/// Background worker that handles Request/Reply pattern processing.
///
/// WHY: This worker simulates a responder service. It listens on the
/// request queue, processes incoming requests (echo or arithmetic),
/// and sends responses to the reply queue. In real applications,
/// this would be a separate service that performs actual work.
/// </summary>
public class ReplyHandler : BackgroundService
{
    private readonly ILogger<ReplyHandler> _logger;
    private readonly IMqConnectionService _mqConnection;
    private readonly RequestReplyService _requestReplyService;
    private readonly IQueueBrowserService _queueBrowser;

    public ReplyHandler(
        ILogger<ReplyHandler> logger,
        IMqConnectionService mqConnection,
        RequestReplyService requestReplyService,
        IQueueBrowserService queueBrowser)
    {
        _logger = logger;
        _mqConnection = mqConnection;
        _requestReplyService = requestReplyService;
        _queueBrowser = queueBrowser;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reply handler starting...");

        // Wait for MQ connection
        await Task.Delay(4000, stoppingToken);

        // Run two tasks: one to handle requests, one to handle replies
        var requestTask = HandleRequests(stoppingToken);
        var replyTask = HandleReplies(stoppingToken);

        await Task.WhenAny(requestTask, replyTask);

        _logger.LogInformation("Reply handler stopped.");
    }

    private async Task HandleRequests(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRequests(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing requests. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessRequests(CancellationToken stoppingToken)
    {
        var queueManager = _mqConnection.GetQueueManager() as MQQueueManager;
        if (queueManager == null || !_mqConnection.IsConnected)
        {
            await Task.Delay(5000, stoppingToken);
            return;
        }

        MQQueue? requestQueue = null;
        MQQueue? replyQueue = null;

        try
        {
            requestQueue = queueManager.AccessQueue(
                PatternConfig.RequestQueue,
                MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);

            _logger.LogInformation("Listening for requests on: {Queue}", PatternConfig.RequestQueue);

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

                    requestQueue.Get(mqMessage, getOptions);

                    var content = mqMessage.ReadString(mqMessage.MessageLength);
                    var replyToQueue = mqMessage.ReplyToQueueName;
                    var correlationId = mqMessage.MessageId;
                    var requestMessageId = Guid.NewGuid().ToString();

                    _logger.LogInformation("Request received: {Content}", content);

                    // Wait for visualization delay before processing
                    await Task.Delay(_queueBrowser.ConsumptionDelayMs, stoppingToken);

                    // Notify queue browser that request was consumed
                    await _queueBrowser.NotifyMessageDequeued(requestMessageId, PatternConfig.RequestQueue);

                    // Process the request (simple responder logic)
                    var responseContent = ProcessRequest(content);

                    // Send reply
                    if (!string.IsNullOrEmpty(replyToQueue))
                    {
                        replyQueue = queueManager.AccessQueue(
                            replyToQueue.Trim(),
                            MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);

                        var replyMessage = new MQMessage();
                        replyMessage.WriteString(responseContent);
                        replyMessage.Format = MQC.MQFMT_STRING;
                        replyMessage.CorrelationId = correlationId;
                        replyMessage.MessageType = MQC.MQMT_REPLY;

                        replyQueue.Put(replyMessage, new MQPutMessageOptions());

                        // Notify queue browser that reply was added to queue
                        var replyQueueMessage = new QueueMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Content = responseContent,
                            QueueName = PatternConfig.ReplyQueue,
                            Pattern = MessagePattern.RequestReply,
                            CorrelationId = Convert.ToBase64String(correlationId),
                            EnqueuedAt = DateTime.UtcNow
                        };
                        await _queueBrowser.NotifyMessageEnqueued(replyQueueMessage);

                        replyQueue.Close();
                        replyQueue = null;

                        _logger.LogInformation("Reply sent: {Response}", responseContent);
                    }
                }
                catch (MQException ex) when (ex.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
                {
                    continue;
                }
            }
        }
        finally
        {
            replyQueue?.Close();
            requestQueue?.Close();
        }
    }

    private async Task HandleReplies(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessReplies(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing replies. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessReplies(CancellationToken stoppingToken)
    {
        var queueManager = _mqConnection.GetQueueManager() as MQQueueManager;
        if (queueManager == null || !_mqConnection.IsConnected)
        {
            await Task.Delay(5000, stoppingToken);
            return;
        }

        MQQueue? replyQueue = null;
        try
        {
            replyQueue = queueManager.AccessQueue(
                PatternConfig.ReplyQueue,
                MQC.MQOO_INPUT_AS_Q_DEF | MQC.MQOO_FAIL_IF_QUIESCING);

            _logger.LogInformation("Listening for replies on: {Queue}", PatternConfig.ReplyQueue);

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

                    replyQueue.Get(mqMessage, getOptions);

                    var content = mqMessage.ReadString(mqMessage.MessageLength);
                    var correlationId = mqMessage.CorrelationId;
                    var replyMessageId = Guid.NewGuid().ToString();

                    // Wait for visualization delay before processing reply
                    await Task.Delay(_queueBrowser.ConsumptionDelayMs, stoppingToken);

                    // Notify queue browser that reply was consumed
                    await _queueBrowser.NotifyMessageDequeued(replyMessageId, PatternConfig.ReplyQueue);

                    await _requestReplyService.HandleReplyReceived(correlationId, content);
                }
                catch (MQException ex) when (ex.ReasonCode == MQC.MQRC_NO_MSG_AVAILABLE)
                {
                    continue;
                }
            }
        }
        finally
        {
            replyQueue?.Close();
        }
    }

    /// <summary>
    /// Processes the request and generates a response.
    /// Handles echo and arithmetic expressions.
    /// </summary>
    private string ProcessRequest(string content)
    {
        // Check for arithmetic expression: "Calculate: X+Y" or "X+Y"
        var calcMatch = Regex.Match(content, @"(?:Calculate:\s*)?(\d+)\s*\+\s*(\d+)", RegexOptions.IgnoreCase);
        if (calcMatch.Success)
        {
            var a = int.Parse(calcMatch.Groups[1].Value);
            var b = int.Parse(calcMatch.Groups[2].Value);
            return (a + b).ToString();
        }

        var subMatch = Regex.Match(content, @"(?:Calculate:\s*)?(\d+)\s*-\s*(\d+)", RegexOptions.IgnoreCase);
        if (subMatch.Success)
        {
            var a = int.Parse(subMatch.Groups[1].Value);
            var b = int.Parse(subMatch.Groups[2].Value);
            return (a - b).ToString();
        }

        var mulMatch = Regex.Match(content, @"(?:Calculate:\s*)?(\d+)\s*\*\s*(\d+)", RegexOptions.IgnoreCase);
        if (mulMatch.Success)
        {
            var a = int.Parse(mulMatch.Groups[1].Value);
            var b = int.Parse(mulMatch.Groups[2].Value);
            return (a * b).ToString();
        }

        // Default: echo the message
        return $"Echo: {content}";
    }
}
