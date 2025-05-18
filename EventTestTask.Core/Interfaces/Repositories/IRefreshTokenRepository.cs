using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.Entities;

namespace EventTestTask.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<string> CreateAsync(User user, string refreshToken, DateTime expiration, CancellationToken cancellationToken);

    Task<(string, DateTime)> GetByUserId(Guid userId, CancellationToken cancellationToken);
    
    Task<(string, DateTime)> GetByToken(string refreshToken, CancellationToken cancellationToken);
    
    Task DeleteByTokenAsync(string refreshToken, CancellationToken cancellationToken);
}