using AutoMapper;
using EventTestTask.Core.DTOs.Event;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Filters;
using EventTestTask.Core.Models.Pagination;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Interfaces.Repositories;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EventTestTask.Application.Services;

public class EventsService : IEventsService
{
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EventsService> _logger;
    private readonly IEventsRepository _eventsRepository;
    private readonly IValidator<EventRequest> _eventValidator;

    public EventsService(IEventsRepository eventsRepository, IMapper mapper, IValidator<EventRequest> eventValidator,
        IMemoryCache cache, ILogger<EventsService> logger)
    {
        _mapper = mapper;
        _eventsRepository = eventsRepository;
        _eventValidator = eventValidator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PageResult<EventResponse>> GetEvents(PageParams pageParams, CancellationToken cancellationToken)
    {
        var events = await _eventsRepository.GetEventsAsync(pageParams, cancellationToken);
        return _mapper.Map<PageResult<EventResponse>>(events);
    }

    public async Task<EventResponse> GetEventById(Guid eventId, CancellationToken cancellationToken)
    {
        var @event = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        return _mapper.Map<EventResponse>(@event);
    }

    public async Task<EventResponse> GetEventByTitle(string title, CancellationToken cancellationToken)
    {
        var @event = await _eventsRepository.GetEventByTitleAsync(title, cancellationToken);
        return _mapper.Map<EventResponse>(@event);
    }

    public async Task CreateEvent(EventRequest @event, CancellationToken cancellationToken)
    {
        await _eventValidator.ValidateAndThrowAsync(@event, cancellationToken);
        await _eventsRepository.CreateEventAsync(_mapper.Map<Event>(@event), cancellationToken);
    }

    public async Task UpdateEvent(Guid eventId, EventRequest @event, CancellationToken cancellationToken)
    {
        _cache.Remove($"image_{eventId}".ToString());
        await _eventValidator.ValidateAndThrowAsync(@event, cancellationToken);
        await _eventsRepository.UpdateEventAsync(eventId, _mapper.Map<Event>(@event), cancellationToken);
    }

    public async Task<Guid> DeleteEvent(Guid eventId, CancellationToken cancellationToken)
    {
        _cache.Remove($"image_{eventId}".ToString());
        return await _eventsRepository.DeleteEventAsync(eventId, cancellationToken);
    }

    public async Task<PageResult<EventResponse>> SearchEvents(PageParams pageParams, EventFilter filter,
        CancellationToken cancellationToken)
    {
        var events = await _eventsRepository.SearchEventsAsync(pageParams, filter, cancellationToken);
        return _mapper.Map<PageResult<EventResponse>>(events);
    }

    public async Task UploadEventImage(Guid eventId, byte[] image, CancellationToken cancellationToken)
    {
        _cache.Remove($"image_{eventId}".ToString());
        await _eventsRepository.UploadEventImageAsync(eventId, image, cancellationToken);
    }

    public async Task<byte[]> GetImageByEventId(Guid eventId, CancellationToken cancellationToken)
    {
        var cacheKey = $"image_{eventId}";

        if (!_cache.TryGetValue(cacheKey, out byte[]? image) || image == null)
        {
            image = await _eventsRepository.GetImageByEventIdAsync(eventId, cancellationToken);

            if (image == null || image.Length == 0)
                throw new InvalidOperationException("Image for event not found");

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                .SetPriority(CacheItemPriority.Normal)
                .SetSize(image.Length);

            _cache.Set(cacheKey, image, cacheOptions);
        }
        else
        {
            _logger.LogInformation($"image {eventId} has been retrieved from cache");
        }

        return image;
    }
}