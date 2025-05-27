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
    private readonly IMemoryCache _cache;
    private readonly ILogger<EventsService> _logger;
    private readonly IEventsRepository _eventsRepository;
    private readonly IValidator<Event> _eventValidator;

    public EventsService(IEventsRepository eventsRepository, IValidator<Event> eventValidator,
        IMemoryCache cache, ILogger<EventsService> logger)
    {
        _eventsRepository = eventsRepository;
        _eventValidator = eventValidator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PageResult<Event>> GetEvents(PageParams pageParams, CancellationToken cancellationToken)
    {
        return await _eventsRepository.GetEventsAsync(pageParams, cancellationToken);
    }

    public async Task<Event> GetEventById(Guid eventId, CancellationToken cancellationToken)
    {
        var @event = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        if (@event is null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        return @event;
    }

    public async Task<Event> GetEventByTitle(string title, CancellationToken cancellationToken)
    {
        var @event = await _eventsRepository.GetEventByTitleAsync(title, cancellationToken);
        if (@event is null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        return @event;
    }

    public async Task CreateEvent(Event @event, CancellationToken cancellationToken)
    {
        await _eventValidator.ValidateAndThrowAsync(@event, cancellationToken);
        await _eventsRepository.CreateEventAsync(@event, cancellationToken);
    }

    public async Task UpdateEvent(Guid eventId, Event @event, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        if (existingEvent is null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        await _eventValidator.ValidateAndThrowAsync(@event, cancellationToken);
        await _eventsRepository.UpdateEventAsync(eventId, @event, cancellationToken);
        _cache.Remove($"image_{eventId}".ToString());
    }

    public async Task<Guid> DeleteEvent(Guid eventId, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        if (existingEvent is null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        _cache.Remove($"image_{eventId}".ToString());
        return await _eventsRepository.DeleteEventAsync(eventId, cancellationToken);
    }

    public async Task<PageResult<Event>> SearchEvents(PageParams pageParams, EventFilter filter,
        CancellationToken cancellationToken)
    {
        return await _eventsRepository.SearchEventsAsync(pageParams, filter, cancellationToken);
    }

    public async Task UploadEventImage(Guid eventId, byte[] image, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        if (existingEvent is null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        _cache.Remove($"image_{eventId}".ToString());
        await _eventsRepository.UploadEventImageAsync(eventId, image, cancellationToken);
    }

    public async Task<byte[]> GetImageByEventId(Guid eventId, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        if (existingEvent is null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        var cacheKey = $"image_{eventId}";

        if (!_cache.TryGetValue(cacheKey, out byte[]? image) || image == null)
        {
            image = await _eventsRepository.GetImageByEventIdAsync(eventId, cancellationToken);

            if (image == null || image.Length == 0)
            {
                throw new KeyNotFoundException("Image for event not found");
            }

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