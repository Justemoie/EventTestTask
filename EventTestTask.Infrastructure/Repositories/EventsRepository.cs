using EventTestTask.Core.Entities;
using Microsoft.EntityFrameworkCore;
using EventTestTask.Core.Models.Filters;
using EventTestTask.Core.Models.Pagination;
using EventTestTask.Infrastructure.Extensions;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Infrastructure.ApplicationContext;

namespace EventTestTask.Infrastructure.Repositories;

public class EventsRepository : IEventsRepository
{
    private readonly AppDbContext _context;

    public EventsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PageResult<Event>> GetEvents(PageParams pageParams, CancellationToken cancellationToken)
    {
        var query = _context.Events.AsQueryable();

        var events = await query.ToPage(pageParams, cancellationToken);

        return events;
    }

    public async Task<Event> GetEventById(Guid eventId, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == eventId, cancellationToken);

        if (@event is null)
            throw new KeyNotFoundException("Event not found");

        return @event;
    }

    public async Task<Event> GetEventByTitle(string title, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Title == title, cancellationToken);

        if (@event is null)
            throw new KeyNotFoundException("Event not found");

        return @event;
    }

    public async Task CreateEvent(Event @event, CancellationToken cancellationToken)
    {
        var newEvent = new Event(
            Guid.NewGuid(),
            @event.Title,
            @event.Description,
            @event.StartDate,
            @event.EndDate,
            @event.Location,
            @event.Category,
            @event.MaxParticipants,
            @event.Image
        );

        await _context.AddAsync(newEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateEvent(Guid eventId, Event @event, CancellationToken cancellationToken)
    {
        await _context.Events.Where(e => e.Id == eventId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Title, @event.Title)
                .SetProperty(e => e.Description, @event.Description)
                .SetProperty(e => e.StartDate, @event.StartDate)
                .SetProperty(e => e.EndDate, @event.EndDate)
                .SetProperty(e => e.Location, @event.Location)
                .SetProperty(e => e.Category, @event.Category)
                .SetProperty(e => e.MaxParticipants, @event.MaxParticipants)
                .SetProperty(e => e.Image, @event.Image), cancellationToken);
    }

    public async Task<Guid> DeleteEvent(Guid eventId, CancellationToken cancellationToken)
    {
        var eventToDelete = await _context.Events
            .FindAsync(eventId, cancellationToken);

        if (eventToDelete is null)
            throw new KeyNotFoundException("Event not found");

        _context.Events.Remove(eventToDelete);
        await _context.SaveChangesAsync(cancellationToken);

        return eventToDelete.Id;
    }

    public async Task<PageResult<Event>> SearchEvents(
        PageParams pageParams,
        EventFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _context.Events.AsQueryable();

        var events = await query
            .Filter(filter)
            .AsNoTracking()
            .ToPage(pageParams, cancellationToken);

        return events;
    }

    public async Task UploadEventImage(Guid eventId, byte[] image, CancellationToken cancellationToken)
    {
        await _context.Events
            .Where(x => x.Id == eventId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Image, image), cancellationToken);
    }

    public async Task<byte[]> GetImageByEventId(Guid eventId, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        
        if(@event is null)
            throw new KeyNotFoundException("Event not found");
        
        return @event.Image;
    }
}