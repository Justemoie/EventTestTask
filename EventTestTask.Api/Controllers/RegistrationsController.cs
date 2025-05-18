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

    [HttpGet("participants/{eventId:guid}")]
    [Authorize]
    public async Task<ActionResult<List<UserResponse>?>> GetParticipants(
        [FromQuery] PageParams pageParams, [FromRoute] Guid eventId,
        CancellationToken cancellationToken)
    {
        var participants = await _registrationsService.GetParticipants(pageParams, eventId, cancellationToken);

        return Ok(participants);
    }

    [HttpGet("events/{eventId:guid}/users/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<UserResponse?>> GetParticipant([FromRoute] Guid eventId, [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        var participant = await _registrationsService.GetParticipantById(eventId, userId, cancellationToken);

        return Ok(participant);
    }

    [HttpPost("join/events/{eventId:guid}/users/{userId:guid}")]
    [Authorize]
    public async Task<IActionResult> Join([FromRoute] Guid eventId, [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        await _registrationsService.JoinToEvent(eventId, userId, cancellationToken);

        return Ok();
    }

    [HttpPost("leave/events/{eventId:guid}/users/{userId:guid}")]
    [Authorize]
    public async Task<IActionResult> Leave([FromRoute] Guid eventId, [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        await _registrationsService.LeaveFromEvent(eventId, userId, cancellationToken);

        return Ok();
    }
}