using EventTestTask.Core.DTOs.Event;
using EventTestTask.Core.Models.Filters;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Interfaces.Services;

public interface IEventsService
{
    Task<PageResult<EventResponse>> GetEvents(PageParams pageParams, CancellationToken cancellationToken);
    
    Task<EventResponse> GetEventById(Guid eventId, CancellationToken cancellationToken);
    
    Task<EventResponse> GetEventByTitle(string title, CancellationToken cancellationToken);
    
    Task CreateEvent(EventRequest @event, CancellationToken cancellationToken);

    Task UpdateEvent(Guid eventId, EventRequest @event, CancellationToken cancellationToken);

    Task<Guid> DeleteEvent(Guid eventId, CancellationToken cancellationToken);

    Task<PageResult<EventResponse>> SearchEvents(PageParams pageParams, EventFilter filter,
        CancellationToken cancellationToken);

    Task UploadEventImage(Guid eventId, byte[] image, CancellationToken cancellationToken);
    
    Task<byte[]> GetImageByEventId(Guid eventId, CancellationToken cancellationToken);
}