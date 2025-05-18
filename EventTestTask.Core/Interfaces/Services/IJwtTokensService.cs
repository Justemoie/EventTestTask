using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace EventTestTask.Core.Interfaces.Services;

public interface IJwtTokensService
{
    Task<TokenResponse> GenerateTokens(User user, CancellationToken cancellationToken);
    Task<TokenResponse> UpdateTokens(HttpContext httpContext, CancellationToken cancellationToken);
    Task InvalidateRefreshToken(string refreshToken, CancellationToken cancellationToken);
}