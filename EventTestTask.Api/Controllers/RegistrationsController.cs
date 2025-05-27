using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTestTask.Api.Controllers;

[ApiController]
[Route("api/registrations")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationsService _registrationsService;

    public RegistrationsController(IRegistrationsService registrationsService)
    {
        _registrationsService = registrationsService;
    }

    [HttpGet("{eventId:guid}")]
    [Authorize]
    public async Task<ActionResult<List<UserResponse>?>> GetEventParticipants(
        [FromQuery] PageParams pageParams, [FromRoute] Guid eventId,
        CancellationToken cancellationToken)
    {
        var participants = await _registrationsService.GetEventParticipants(pageParams, eventId, cancellationToken);
        return Ok(participants);
    }

    [HttpGet("{eventId:guid}/users/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<UserResponse?>> GetEventParticipant([FromRoute] Guid eventId, [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var participant = await _registrationsService.GetEventParticipantById(eventId, userId, cancellationToken);
        return Ok(participant);
    }

    [HttpPost("{eventId:guid}/users/{userId:guid}/register")]
    [Authorize]
    public async Task<IActionResult> RegisterForEvent([FromRoute] Guid eventId, [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        await _registrationsService.RegisterForEvent(eventId, userId, cancellationToken);
        return Ok();
    }

    [HttpPost("{eventId:guid}/users/{userId:guid}/unregister")]
    [Authorize]
    public async Task<IActionResult> UnregisterFromEvent([FromRoute] Guid eventId, [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        await _registrationsService.UnregisterFromEvent(eventId, userId, cancellationToken);
        return Ok();
    }
}