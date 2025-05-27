using AutoMapper;
using EventTestTask.Core.DTOs.Event;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Models.Filters;
using EventTestTask.Core.Models.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventTestTask.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventsService _eventsService;
    private readonly IMapper _mapper;
    
    public EventsController(IEventsService eventsService, IMapper mapper)
    {
        _eventsService = eventsService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<EventResponse>>> GetEvents(
        [AsParameters] [FromQuery] PageParams pageParams,
        CancellationToken cancellationToken)
    {
        var events = await _eventsService.GetEvents(pageParams, cancellationToken);
        var eventsResponse = _mapper.Map<PageResult<EventResponse>>(events);
        return Ok(eventsResponse);
    }

    [HttpGet("{eventId:guid}")]
    public async Task<ActionResult<EventResponse>> GetEventById([FromRoute] Guid eventId,
        CancellationToken cancellationToken)
    {
        var @event = await _eventsService.GetEventById(eventId, cancellationToken);
        var eventResponse = _mapper.Map<EventResponse>(@event);
        return Ok(@eventResponse);
    }

    [HttpGet("by-title")]
    public async Task<ActionResult<EventResponse>> GetEventByTitle([FromQuery] string title,
        CancellationToken cancellationToken)
    {
        var @event = await _eventsService.GetEventByTitle(title, cancellationToken);
        var eventResponse = _mapper.Map<EventResponse>(@event);
        return Ok(@eventResponse);
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEvent([FromBody] EventRequest eventRequest,
        CancellationToken cancellationToken)
    {
        var @event = _mapper.Map<Event>(eventRequest);
        await _eventsService.CreateEvent(@event, cancellationToken);
        return Ok();
    }

    [HttpPut("update/{eventId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEvent([FromRoute] Guid eventId,
        [FromBody] EventRequest eventRequest,
        CancellationToken cancellationToken)
    {
        var @event = _mapper.Map<Event>(eventRequest);
        await _eventsService.UpdateEvent(eventId, @event, cancellationToken);
        return Ok();
    }

    [HttpDelete("delete/{eventId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Guid>> DeleteEvent([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var id = await _eventsService.DeleteEvent(eventId, cancellationToken);
        return Ok(id);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PageResult<EventResponse>>> SearchEvents(
        [FromQuery] PageParams pageParams,
        [FromQuery] EventFilter filter,
        CancellationToken cancellationToken)
    {
        var events = await _eventsService.SearchEvents(pageParams, filter, cancellationToken);
        var eventsResponse = _mapper.Map<PageResult<EventResponse>>(events);
        return Ok(eventsResponse);
    }

    [HttpGet("image/{eventId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<byte[]>> GetImageByEventId([FromRoute] Guid eventId,
        CancellationToken cancellationToken)
    {
        var image = await _eventsService.GetImageByEventId(eventId, cancellationToken);
        return Ok(image);
    }

    [HttpPatch("image-update/{eventId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadEventImage([FromRoute] Guid eventId, byte[] image,
        CancellationToken cancellationToken)
    {
        await _eventsService.UploadEventImage(eventId, image, cancellationToken);
        return Ok();
    }
}