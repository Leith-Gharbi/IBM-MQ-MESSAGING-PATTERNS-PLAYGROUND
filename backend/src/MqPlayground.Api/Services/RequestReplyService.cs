using System.Collections.Concurrent;
using IBM.WMQ;
using Microsoft.AspNetCore.SignalR;
using MqPlayground.Api.Hubs;
using MqPlayground.Api.Models;

namespace MqPlayground.Api.Services;

/// <summary>
/// Service for Request/Reply messaging pattern.
///
/// WHY: Request/Reply demonstrates synchronous communication over MQ.
/// A requester sends a message to a request queue with a correlation ID
/// and reply queue name. The responder processes the request and sends
/// the reply to the specified queue with matching correlation ID.
/// This enables request-response patterns over asynchronous messaging.
/// </summary>
public class RequestReplyService
{
    private readonly ILogger<RequestReplyService> _logger;
    private readonly IMqConnectionService _mqConnection;
    private readonly IHubContext<MessageHub> _hubContext;
    private readonly ConcurrentDictionary<string, Request> _pendingRequests = new();

    public RequestReplyService(
        ILogger<RequestReplyService> logger,
        IMqConnectionService mqConnection,
        IHubContext<MessageHub> hubContext)
    {
        _logger = logger;
        _mqConnection = mqConnection;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Sends a request and tracks it for reply.
    /// </summary>
    /// <param name="content">The request content.</param>
    /// <param name="timeoutSeconds">Reply timeout in seconds.</param>
    /// <returns>The pending request object.</returns>
    public Request SendRequest(string content, int timeoutSeconds = 30)
    {
        var queueManager = _mqConnection.GetQueueManager() as MQQueueManager;
        if (queueManager == null || !_mqConnection.IsConnected)
        {
            throw new InvalidOperationException("Not connected to MQ");
        }

        var request = new Request
        {
            Content = content,
            TimeoutSeconds = timeoutSeconds,
            SentAt = DateTime.UtcNow,
            Status = RequestStatus.Pending
        };

        MQQueue? requestQueue = null;
        try
        {
            // Open request queue for output
            requestQueue = queueManager.AccessQueue(
                PatternConfig.RequestQueue,
                MQC.MQOO_OUTPUT | MQC.MQOO_FAIL_IF_QUIESCING);

            // Create MQ message with reply-to queue
            var mqMessage = new MQMessage();
            mqMessage.WriteString(content);
            mqMessage.Format = MQC.MQFMT_STRING;
            mqMessage.ReplyToQueueName = PatternConfig.ReplyQueue;
            mqMessage.MessageType = MQC.MQMT_REQUEST;

            // Put message to request queue
            var putOptions = new MQPutMessageOptions();
            requestQueue.Put(mqMessage, putOptions);

            // Store correlation ID (MQ sets MessageId on Put)
            request.CorrelationId = Convert.ToBase64String(mqMessage.MessageId);

            // Track the pending request
            _pendingRequests.TryAdd(request.CorrelationId, request);

            _logger.LogInformation(
                "Request sent with correlation ID: {CorrelationId}",
                request.CorrelationId);

            // Start timeout timer
            _ = StartTimeoutTimer(request);

            return request;
        }
        catch (MQException ex)
        {
            _logger.LogError(ex,
                "Failed to send request. Reason code: {ReasonCode}",
                ex.ReasonCode);
            throw;
        }
        finally
        {
            requestQueue?.Close();
        }
    }

    /// <summary>
    /// Handles a reply received for a pending request.
    /// Called by ReplyHandler worker.
    /// </summary>
    public async Task HandleReplyReceived(byte[] correlationId, string content)
    {
        var correlationIdString = Convert.ToBase64String(correlationId);

        if (!_pendingRequests.TryRemove(correlationIdString, out var request))
        {
            _logger.LogWarning("Received reply for unknown request: {CorrelationId}", correlationIdString);
            return;
        }

        var response = new Message
        {
            Content = content,
            Direction = MessageDirection.Received,
            Pattern = MessagePattern.RequestReply,
            CorrelationId = correlationIdString,
            Timestamp = DateTime.UtcNow
        };

        request.Status = RequestStatus.Completed;
        request.Response = response;

        _logger.LogInformation(
            "Reply received for correlation ID: {CorrelationId}",
            correlationIdString);

        // Notify clients
        await _hubContext.Clients.Group("Playground")
            .SendAsync("RequestStatusChanged", request);
    }

    private async Task StartTimeoutTimer(Request request)
    {
        await Task.Delay(TimeSpan.FromSeconds(request.TimeoutSeconds));

        if (_pendingRequests.TryRemove(request.CorrelationId, out var timedOutRequest))
        {
            timedOutRequest.Status = RequestStatus.TimedOut;

            _logger.LogWarning(
                "Request timed out: {CorrelationId}",
                request.CorrelationId);

            await _hubContext.Clients.Group("Playground")
                .SendAsync("RequestStatusChanged", timedOutRequest);
        }
    }
}
