using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.DTOs.User;

namespace EventTestTask.Core.Interfaces.Services;

public interface IUsersService
{
    Task<UserResponse> GetUserById(Guid userId, CancellationToken cancellationToken);
    Task<UserResponse> GetByEmail(string email, CancellationToken cancellationToken);
    
    Task Register(RegisterUser user, CancellationToken cancellationToken);
    Task UpdateUser(Guid userId, UserRequest userRequest, CancellationToken cancellationToken);
    Task<TokenResponse> Login(string email, string password, CancellationToken cancellationToken);
    Task Logout(string refreshToken,CancellationToken cancellationToken);
}