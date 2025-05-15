using EventTestTask.Core.DTOs.Event;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Models.Filters;
using EventTestTask.Core.Models.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace EventTestTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventsService _eventsService;

    public EventsController(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [HttpGet("/events")]
    public async Task<ActionResult<PageResult<EventResponse>>> GetAll(
        [AsParameters] [FromQuery] PageParams pageParams,
        CancellationToken cancellationToken)
    {
        var events = await _eventsService.GetEvents(pageParams, cancellationToken);

        return Ok(events);
    }

    [HttpGet("/events/{eventId:guid}")]
    public async Task<ActionResult<EventResponse>> GetById([FromRoute] Guid eventId,
        CancellationToken cancellationToken)
    {
        var @event = await _eventsService.GetEventById(eventId, cancellationToken);

        return Ok(@event);
    }

    [HttpGet("/events-by-title")]
    public async Task<ActionResult<EventResponse>> GetByTitle([FromQuery] string title,
        CancellationToken cancellationToken)
    {
        var @event = await _eventsService.GetEventByTitle(title, cancellationToken);

        return Ok(@event);
    }

    [HttpPost("/events")]
    public async Task<IActionResult> Create([FromBody] EventRequest eventRequest,
        CancellationToken cancellationToken)
    {
        await _eventsService.CreateEvent(eventRequest, cancellationToken);

        return Ok();
    }

    [HttpPut("/events/{eventId:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid eventId,
        [FromBody] EventRequest eventRequest,
        CancellationToken cancellationToken)
    {
        await _eventsService.UpdateEvent(eventId, eventRequest, cancellationToken);

        return Ok();
    }

    [HttpDelete("/events/{eventId:guid}")]
    public async Task<ActionResult<Guid>> Delete([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var id = await _eventsService.DeleteEvent(eventId, cancellationToken);

        return Ok(id);
    }

    [HttpGet("/search/events")]
    public async Task<ActionResult<PageResult<EventResponse>>> Search(
        [AsParameters] [FromQuery] PageParams pageParams,
        [AsParameters] [FromQuery] EventFilter filter,
        CancellationToken cancellationToken)
    {
        var events = await _eventsService.SearchEvents(pageParams, filter, cancellationToken);

        return Ok(events);
    }

    [HttpGet("/events/image/{eventId:guid}")]
    public async Task<ActionResult<byte[]>> GetImageByEventId([FromRoute] Guid eventId,
        CancellationToken cancellationToken)
    {
        var image = await _eventsService.GetImageByEventId(eventId, cancellationToken);

        return Ok(image);
    }

    [HttpPatch("/events/image-update/{eventId:guid}")]
    public async Task<IActionResult> UploadImage([FromRoute] Guid eventId, byte[] image,
        CancellationToken cancellationToken)
    {
        await _eventsService.UploadEventImage(eventId, image, cancellationToken);

        return Ok();
    }
}