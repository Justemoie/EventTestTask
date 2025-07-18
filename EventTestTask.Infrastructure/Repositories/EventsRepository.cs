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

    public async Task<PageResult<Event>> GetEventsAsync(PageParams pageParams, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .AsNoTracking()
            .AsQueryable()
            .ToPage(pageParams, cancellationToken);
        return events;
    }

    public async Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        return @event;
    }

    public async Task<Event?> GetEventByTitleAsync(string title, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Title == title, cancellationToken);
        return @event;
    }

    public async Task CreateEventAsync(Event @newEvent, CancellationToken cancellationToken)
    {
        await _context.AddAsync(newEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateEventAsync(Guid eventId, Event @event, CancellationToken cancellationToken)
    {
        await _context.Events
            .Where(e => e.Id == eventId)
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

    public async Task<Guid> DeleteEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        await _context.Events
            .Where(e => e.Id == eventId)
            .ExecuteDeleteAsync(cancellationToken);
        return eventId;
    }

    public async Task<PageResult<Event>> SearchEventsAsync(
        PageParams pageParams, EventFilter filter, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .AsNoTracking()
            .AsQueryable()
            .Filter(filter)
            .ToPage(pageParams, cancellationToken);
        return events;
    }

    public async Task UploadEventImageAsync(Guid eventId, byte[] image, CancellationToken cancellationToken)
    {
        await _context.Events
            .Where(x => x.Id == eventId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.Image, image), cancellationToken);
    }

    public async Task<byte[]?> GetImageByEventIdAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == eventId, cancellationToken);
        return @event?.Image;
    }
}