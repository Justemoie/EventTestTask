using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Filters;
using EventTestTask.Core.Models.Pagination;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Interfaces.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EventTestTask.Application.Services;

public class EventsService : IEventsService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<EventsService> _logger;
    private readonly IEventsRepository _eventsRepository;
    private readonly IValidator<Event> _eventValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EventsService(IEventsRepository eventsRepository, IValidator<Event> eventValidator,
        IMemoryCache cache, ILogger<EventsService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _eventsRepository = eventsRepository;
        _eventValidator = eventValidator;
        _cache = cache;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
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

        var ev = new Event(
            Guid.NewGuid(),
            @event.CreatorId,
            @event.Title,
            @event.Description,
            DateTime.SpecifyKind(@event.StartDate, DateTimeKind.Utc),
            DateTime.SpecifyKind(@event.EndDate, DateTimeKind.Utc),
            @event.Location,
            @event.Category,
            @event.MaxParticipants,
            @event.Image
        );

        await _eventsRepository.CreateEventAsync(ev, cancellationToken);
    }

    public async Task UpdateEvent(Guid eventId, Event @event, CancellationToken cancellationToken)
{
    var existingEvent = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
    if (existingEvent is null)
    {
        throw new KeyNotFoundException("Event not found");
    }
    
    // Получаем JWT токен из cookies
    var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["_at"]; // или другое имя вашего cookie
    
    if (string.IsNullOrEmpty(jwtToken))
    {
        throw new UnauthorizedAccessException("JWT token not found");
    }
    
    // Декодируем JWT токен
    var tokenHandler = new JwtSecurityTokenHandler();
    var securityToken = tokenHandler.ReadJwtToken(jwtToken);
    
    // Извлекаем данные из claims
    var userRole = securityToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value;
    var userId = securityToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
    
    if (userRole is "Admin" || userId == @existingEvent.CreatorId.ToString())
    {
        Console.WriteLine($"image: {@event.Image}");
        await _eventValidator.ValidateAndThrowAsync(@event, cancellationToken);
    
        var ev = new Event()
        {
            Id = eventId,
            CreatorId = @event.CreatorId,
            Title = @event.Title,
            Description = @event.Description,
            StartDate = DateTime.SpecifyKind(@event.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(@event.EndDate, DateTimeKind.Utc),
            Location = @event.Location,
            Category = @event.Category,
            MaxParticipants = @event.MaxParticipants,
            Image = @event.Image
        };
    
        await _eventsRepository.UpdateEventAsync(eventId, ev, cancellationToken);
        _cache.Remove($"image_{eventId}".ToString());
    }
    else
    {
        throw new UnauthorizedAccessException("You don't have permission to update this event");
    }
}

    public async Task<Guid> DeleteEvent(Guid eventId, CancellationToken cancellationToken)
    {
        var existingEvent = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        if (existingEvent is null)
        {
            throw new KeyNotFoundException("Event not found");
        }
        
        var userRole = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == "Role")?.Value;
        var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == "UserId")?.Value;

        if (userRole is "Admin" || userId == existingEvent.CreatorId.ToString())
        {
            _cache.Remove($"image_{eventId}".ToString());
            return await _eventsRepository.DeleteEventAsync(eventId, cancellationToken);
        }
        
        throw new UnauthorizedAccessException("You do not have permission to delete this event");
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
    
    public async Task<PageResult<Event>> GetEventsCreatedByUser(Guid userId, PageParams pageParams, CancellationToken ct)
    {
        return await _eventsRepository.GetEventsCreatedByUser(userId, pageParams, ct);
    }

    public async Task<PageResult<Event>> GetEventsRegisteredByUser(Guid userId, PageParams pageParams, CancellationToken ct)
    {
        return await _eventsRepository.GetEventsRegisteredByUser(userId, pageParams, ct);
    }
}