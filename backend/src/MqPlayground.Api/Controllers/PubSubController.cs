using Microsoft.AspNetCore.Mvc;
using MqPlayground.Api.Models;
using MqPlayground.Api.Services;

namespace MqPlayground.Api.Controllers;

/// <summary>
/// Controller for Publish/Subscribe messaging pattern endpoints.
/// </summary>
[ApiController]
[Route("api/pubsub")]
public class PubSubController : ControllerBase
{
    private readonly ILogger<PubSubController> _logger;
    private readonly PubSubService _pubSubService;

    public PubSubController(
        ILogger<PubSubController> logger,
        PubSubService pubSubService)
    {
        _logger = logger;
        _pubSubService = pubSubService;
    }

    /// <summary>
    /// Publishes a message to the Pub/Sub topic.
    /// </summary>
    [HttpPost("publish")]
    [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<Message> PublishMessage([FromBody] SendMessageRequest request)
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
            var message = _pubSubService.PublishMessage(request.Content);
            return Ok(message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "MQ not connected");
            return StatusCode(503, ErrorResponse.Create("MqUnavailable", "MQ service unavailable"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message");
            return StatusCode(503, ErrorResponse.Create("PublishFailed", ex.Message));
        }
    }

    /// <summary>
    /// Gets the list of current subscribers.
    /// </summary>
    [HttpGet("subscribers")]
    [ProducesResponseType(typeof(IEnumerable<Subscriber>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Subscriber>> GetSubscribers()
    {
        return Ok(_pubSubService.GetSubscribers());
    }

    /// <summary>
    /// Adds a new subscriber.
    /// </summary>
    [HttpPost("subscribers")]
    [ProducesResponseType(typeof(Subscriber), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<Subscriber> AddSubscriber()
    {
        var subscriber = _pubSubService.AddSubscriber();
        if (subscriber == null)
        {
            return BadRequest(ErrorResponse.Create("MaxSubscribersReached",
                $"Maximum of {PatternConfig.MaxSubscribers} subscribers reached"));
        }

        return Created($"/api/pubsub/subscribers/{subscriber.Id}", subscriber);
    }

    /// <summary>
    /// Removes a subscriber.
    /// </summary>
    [HttpDelete("subscribers/{subscriberId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult RemoveSubscriber(string subscriberId)
    {
        var subscribers = _pubSubService.GetSubscribers();
        if (!subscribers.Any(s => s.Id == subscriberId))
        {
            return NotFound(ErrorResponse.Create("NotFound", "Subscriber not found"));
        }

        if (!_pubSubService.RemoveSubscriber(subscriberId))
        {
            return BadRequest(ErrorResponse.Create("MinSubscribersRequired",
                $"Minimum of {PatternConfig.MinSubscribers} subscribers required"));
        }

        return NoContent();
    }

    /// <summary>
    /// Clears the Pub/Sub message history.
    /// </summary>
    [HttpPost("clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ClearMessages()
    {
        _logger.LogInformation("Pub/Sub messages cleared");
        return NoContent();
    }
}
