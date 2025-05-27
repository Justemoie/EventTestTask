using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.Entities;

namespace EventTestTask.Core.Interfaces.Services;

public interface IJwtTokensService
{
    Task<TokenResponse> GenerateTokens(User user, CancellationToken cancellationToken);
    Task UpdateTokens(CancellationToken cancellationToken);
    Task InvalidateRefreshToken(string refreshToken, CancellationToken cancellationToken);
}