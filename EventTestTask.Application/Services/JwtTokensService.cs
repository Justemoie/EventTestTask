using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EventTestTask.Application.Authentication;
using EventTestTask.Core.DTOs.Jwt;
using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventTestTask.Application.Services;

public class JwtTokensService : IJwtTokensService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IUsersRepository _usersRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public JwtTokensService(IOptions<JwtOptions> jwtOptions, IUsersRepository usersRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtOptions = jwtOptions.Value;
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<TokenResponse> GenerateTokens(User user, CancellationToken cancellationToken)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        var token = new TokenResponse(
            DateTime.UtcNow.AddDays(_jwtOptions.ExpireDays),
            refreshToken,
            accessToken
        );

        await _refreshTokenRepository.CreateAsync(user, token.RefreshToken, token.Expiration, cancellationToken);

        return token;
    }

    public async Task<TokenResponse> UpdateTokens(HttpContext context, CancellationToken cancellationToken)
    {
        var refreshToken = context.Request.Cookies
            .FirstOrDefault(x => x.Key == "_rt");
        var accessToken = context.Request.Cookies
            .FirstOrDefault(x => x.Key == "_at");

        if (refreshToken.Key == null || accessToken.Key == null)
            throw new AuthenticationException("Access token or Refresh token is missing.");

        var token = await _refreshTokenRepository.GetByToken(refreshToken.Value, cancellationToken);

        if (token.Item2 < DateTime.UtcNow)
            throw new AuthenticationException("The refresh token has expired");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken.Value);

        var userId = Guid.Parse(jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value);
        var user = await _usersRepository.GetUserByIdAsync(userId, cancellationToken);

        return await GenerateTokens(user, cancellationToken);
    }

    private string GenerateAccessToken(User user)
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        ];

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddHours(_jwtOptions.ExpireMinutes));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public async Task InvalidateRefreshToken(string refreshToken, CancellationToken cancellationToken) =>
        await _refreshTokenRepository.DeleteByTokenAsync(refreshToken, cancellationToken);
}