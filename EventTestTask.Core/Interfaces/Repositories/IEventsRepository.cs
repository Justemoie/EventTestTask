using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Filters;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Interfaces.Repositories;

public interface IEventsRepository
{
    Task<PageResult<Event>> GetEvents(PageParams pageParams, CancellationToken cancellationToken);

    Task<Event> GetEventById(Guid eventId, CancellationToken cancellationToken);

    Task<Event> GetEventByTitle(string title, CancellationToken cancellationToken);

    Task CreateEvent(Event @event, CancellationToken cancellationToken);

    Task UpdateEvent(Guid eventId, Event @event, CancellationToken cancellationToken);

    Task<Guid> DeleteEvent(Guid eventId, CancellationToken cancellationToken);

    Task<PageResult<Event>> SearchEvents(PageParams pageParams, EventFilter filter,
        CancellationToken cancellationToken);

    Task UploadEventImage(Guid eventId, byte[] image, CancellationToken cancellationToken);
    
    Task<byte[]> GetImageByEventId(Guid eventId, CancellationToken cancellationToken);
}