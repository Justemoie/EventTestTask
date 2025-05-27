using EventTestTask.Core.Entities;

namespace EventTestTask.Core.Interfaces.Repositories;

public interface IUsersRepository
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    
    Task CreateUserAsync(User user, CancellationToken cancellationToken);
    Task UpdateUserAsync(Guid userId, User user, CancellationToken cancellationToken);
}