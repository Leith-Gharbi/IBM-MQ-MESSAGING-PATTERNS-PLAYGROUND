using Microsoft.AspNetCore.Mvc;
using MqPlayground.Api.Models;
using MqPlayground.Api.Services;

namespace MqPlayground.Api.Controllers;

/// <summary>
/// Controller for Point-to-Point messaging pattern endpoints.
/// </summary>
[ApiController]
[Route("api/point-to-point")]
public class PointToPointController : ControllerBase
{
    private readonly ILogger<PointToPointController> _logger;
    private readonly PointToPointService _p2pService;

    public PointToPointController(
        ILogger<PointToPointController> logger,
        PointToPointService p2pService)
    {
        _logger = logger;
        _p2pService = p2pService;
    }

    /// <summary>
    /// Sends a message to the Point-to-Point queue.
    /// </summary>
    /// <param name="request">The message content to send.</param>
    /// <returns>The sent message.</returns>
    [HttpPost("send")]
    [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<Message>> SendMessage([FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ErrorResponse.Create("InvalidContent", "Message content cannot be empty"));
        }

        if (request.Content.Length > 10000)
        {
            return BadRequest(ErrorResponse.Create("ContentTooLong", "Message content exceeds 10,000 characters"));
        }

        try
        {
            var message = await _p2pService.SendMessageAsync(request.Content);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "MQ not connected");
            return StatusCode(503, ErrorResponse.Create("MqUnavailable", "MQ service unavailable"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message");
            return StatusCode(503, ErrorResponse.Create("SendFailed", ex.Message));
        }
    }

    /// <summary>
    /// Clears the Point-to-Point message history.
    /// </summary>
    [HttpPost("clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ClearMessages()
    {
        // Message history is maintained on the client side,
        // this endpoint just signals the clear action
        _logger.LogInformation("Point-to-Point messages cleared");
        return NoContent();
    }
}

/// <summary>
/// Request model for sending messages.
/// </summary>
public class SendMessageRequest
{
    /// <summary>
    /// The message content to send.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
