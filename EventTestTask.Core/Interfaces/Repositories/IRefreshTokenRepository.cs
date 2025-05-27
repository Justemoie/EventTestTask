using EventTestTask.Core.Entities;

namespace EventTestTask.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<(string, DateTime)> GetByToken(string refreshToken, CancellationToken cancellationToken);
    Task<(string, DateTime)> GetByUserId(Guid userId, CancellationToken cancellationToken);
    
    Task CreateAsync(User user, RefreshToken newToken, CancellationToken cancellationToken);
    Task DeleteByTokenAsync(string refreshToken, CancellationToken cancellationToken);
}