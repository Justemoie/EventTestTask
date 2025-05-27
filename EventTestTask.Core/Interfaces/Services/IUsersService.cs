using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.Entities;

namespace EventTestTask.Core.Interfaces.Services;

public interface IUsersService
{
    Task<User> GetUserById(Guid userId, CancellationToken cancellationToken);
    Task<User> GetByEmail(string email, CancellationToken cancellationToken);
    
    Task Register(User user, CancellationToken cancellationToken);
    Task UpdateUser(Guid userId, User userRequest, CancellationToken cancellationToken);
    Task<TokenResponse> Login(string email, string password, CancellationToken cancellationToken);
    Task Logout(CancellationToken cancellationToken);
}