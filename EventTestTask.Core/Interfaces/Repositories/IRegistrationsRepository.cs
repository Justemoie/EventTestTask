using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Interfaces.Repositories;

public interface IRegistrationsRepository
{
    Task<PageResult<User>?> GetParticipantsAsync(PageParams pageaParams, Guid eventId, CancellationToken cancellationToken);
    Task<User?> GetParticipantByIdAsync(Guid eventId, Guid userId, CancellationToken cancellationToken);
    
    Task JoinToEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken);
    Task LeaveFromEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken);
}