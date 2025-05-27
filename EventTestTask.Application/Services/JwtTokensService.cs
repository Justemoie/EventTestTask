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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtTokensService(IOptions<JwtOptions> jwtOptions, IUsersRepository usersRepository,
        IRefreshTokenRepository refreshTokenRepository, IHttpContextAccessor httpContextAccessor)
    {
        _jwtOptions = jwtOptions.Value;
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TokenResponse> GenerateTokens(User? user, CancellationToken cancellationToken)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        var token = new TokenResponse(
            DateTime.UtcNow.AddDays(_jwtOptions.ExpireDays),
            refreshToken,
            accessToken
        );
        
        var newToken = new RefreshToken
        {
            UserId = user!.Id,
            Token = refreshToken,
            Expiration = token.Expiration,
        };
        
        await _refreshTokenRepository.CreateAsync(user, newToken, cancellationToken);

        return token;
    }

    public async Task UpdateTokens(CancellationToken cancellationToken)
    {
        var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies
            .FirstOrDefault(x => x.Key == "_rt");
        var accessToken = _httpContextAccessor.HttpContext.Request.Cookies
            .FirstOrDefault(x => x.Key == "_at");
        
        if (refreshToken.Key == null || accessToken.Key == null)
        {
            throw new AuthenticationException("Access token or Refresh token is missing.");
        }

        var token = await _refreshTokenRepository.GetByToken(refreshToken.Value, cancellationToken);
        if (token.Item2 < DateTime.UtcNow)
        {
            throw new AuthenticationException("The refresh token has expired");
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken.Value);
        var userId = Guid.Parse(jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)!.Value);
        var user = await _usersRepository.GetUserByIdAsync(userId, cancellationToken);

        var newToken = await GenerateTokens(user, cancellationToken);
        _httpContextAccessor.HttpContext.Response.Cookies.Append("_at", newToken.AccessToken);
        _httpContextAccessor.HttpContext.Response.Cookies.Append("_rt", newToken.RefreshToken);
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

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task InvalidateRefreshToken(string refreshToken, CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.DeleteByTokenAsync(refreshToken, cancellationToken);
    }
}