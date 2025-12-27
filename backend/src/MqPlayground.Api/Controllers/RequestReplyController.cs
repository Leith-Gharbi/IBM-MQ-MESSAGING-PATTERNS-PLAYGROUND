using Microsoft.AspNetCore.Mvc;
using MqPlayground.Api.Models;
using MqPlayground.Api.Services;

namespace MqPlayground.Api.Controllers;

/// <summary>
/// Controller for Request/Reply messaging pattern endpoints.
/// </summary>
[ApiController]
[Route("api/request-reply")]
public class RequestReplyController : ControllerBase
{
    private readonly ILogger<RequestReplyController> _logger;
    private readonly RequestReplyService _requestReplyService;

    public RequestReplyController(
        ILogger<RequestReplyController> logger,
        RequestReplyService requestReplyService)
    {
        _logger = logger;
        _requestReplyService = requestReplyService;
    }

    /// <summary>
    /// Sends a request and waits for reply.
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<Request> SendRequest([FromBody] SendRequestMessage requestMessage)
    {
        if (string.IsNullOrWhiteSpace(requestMessage.Content))
        {
            return BadRequest(ErrorResponse.Create("InvalidContent", "Request content cannot be empty"));
        }

        if (requestMessage.Content.Length > 10000)
        {
            return BadRequest(ErrorResponse.Create("ContentTooLong", "Request content exceeds 10,000 characters"));
        }

        var timeout = requestMessage.TimeoutSeconds;
        if (timeout < 1 || timeout > 120)
        {
            timeout = PatternConfig.DefaultRequestTimeoutSeconds;
        }

        try
        {
            var request = _requestReplyService.SendRequest(requestMessage.Content, timeout);
            return Ok(request);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "MQ not connected");
            return StatusCode(503, ErrorResponse.Create("MqUnavailable", "MQ service unavailable"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send request");
            return StatusCode(503, ErrorResponse.Create("SendFailed", ex.Message));
        }
    }

    /// <summary>
    /// Clears the Request/Reply message history.
    /// </summary>
    [HttpPost("clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ClearMessages()
    {
        _logger.LogInformation("Request/Reply messages cleared");
        return NoContent();
    }
}

/// <summary>
/// Request model for sending a request.
/// </summary>
public class SendRequestMessage
{
    /// <summary>
    /// The request content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timeout in seconds for reply (1-120, default 30).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
