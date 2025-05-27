using EventTestTask.Core.Entities;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Interfaces.Repositories;

public interface IRegistrationsRepository
{
    Task<PageResult<User>?> GetParticipantsAsync(PageParams pageaParams, Guid eventId, CancellationToken cancellationToken);
    Task<User?> GetParticipantByIdAsync(Guid eventId, Guid userId, CancellationToken cancellationToken);
    
    Task RegisterForEventAsync(Guid eventId, Guid userId, Registration newRegistration, CancellationToken cancellationToken);
    Task UnregisterFromEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken);
    
    Task<bool> CheckUserAndEventExistAsync(Guid userId, Guid eventId, CancellationToken cancellationToken);
    Task<bool> CheckEventExistsAsync(Guid eventId, CancellationToken cancellationToken);
}