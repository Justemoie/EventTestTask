using EventTestTask.Core.DTOs.User;

namespace EventTestTask.Core.Interfaces.Services;

public interface IUsersService
{
    Task<UserResponse> GetUserById(Guid userId, CancellationToken cancellationToken);
    Task<UserResponse> GetByEmail(string email, CancellationToken cancellationToken);
    
    Task CreateUser(UserRequest user, CancellationToken cancellationToken);
    Task UpdateUser(Guid userId, UserRequest user, CancellationToken cancellationToken);
}