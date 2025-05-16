using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Core.Interfaces.Services;

public interface IRegistrationsService
{
    Task<PageResult<UserResponse>?> GetParticipants(PageParams pageaParams, Guid eventId, CancellationToken cancellationToken);
    Task<UserResponse?> GetParticipantById(Guid eventId, Guid userId, CancellationToken cancellationToken);
    
    Task JoinToEvent(Guid eventId, Guid userId, CancellationToken cancellationToken);
    Task LeaveFromEvent(Guid eventId, Guid userId, CancellationToken cancellationToken);
}