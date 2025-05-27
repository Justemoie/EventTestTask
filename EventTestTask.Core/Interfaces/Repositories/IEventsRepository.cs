using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Filters;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Interfaces.Repositories;

public interface IEventsRepository
{
    Task<PageResult<Event>> GetEventsAsync(PageParams pageParams, CancellationToken cancellationToken);
    Task<Event?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken);
    Task<Event?> GetEventByTitleAsync(string title, CancellationToken cancellationToken);
    Task<PageResult<Event>> SearchEventsAsync(PageParams pageParams, EventFilter filter,
        CancellationToken cancellationToken);

    Task CreateEventAsync(Event @event, CancellationToken cancellationToken);
    Task UpdateEventAsync(Guid eventId, Event @event, CancellationToken cancellationToken);
    Task<Guid> DeleteEventAsync(Guid eventId, CancellationToken cancellationToken);

    Task UploadEventImageAsync(Guid eventId, byte[] image, CancellationToken cancellationToken);
    Task<byte[]?> GetImageByEventIdAsync(Guid eventId, CancellationToken cancellationToken);
}