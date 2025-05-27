using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Interfaces.Services;

public interface IRegistrationsService
{
    Task<PageResult<User>?> GetEventParticipants(PageParams pageaParams, Guid eventId, CancellationToken cancellationToken);
    Task<User?> GetEventParticipantById(Guid eventId, Guid userId, CancellationToken cancellationToken);
    
    Task RegisterForEvent(Guid eventId, Guid userId, CancellationToken cancellationToken);
    Task UnregisterFromEvent(Guid eventId, Guid userId, CancellationToken cancellationToken);
}