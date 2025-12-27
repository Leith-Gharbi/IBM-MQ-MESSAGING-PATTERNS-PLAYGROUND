using Microsoft.AspNetCore.Mvc;
using MqPlayground.Api.Models;
using MqPlayground.Api.Services;

namespace MqPlayground.Api.Controllers;

/// <summary>
/// Controller for MQ connection status endpoint.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly IMqConnectionService _mqConnection;

    public StatusController(IMqConnectionService mqConnection)
    {
        _mqConnection = mqConnection;
    }

    /// <summary>
    /// Gets the current MQ connection status.
    /// </summary>
    /// <returns>ConnectionStatus with current connection state.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ConnectionStatus), StatusCodes.Status200OK)]
    public ActionResult<ConnectionStatus> GetStatus()
    {
        return Ok(_mqConnection.GetStatus());
    }
}
