using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Infrastructure.ApplicationContext;
using Microsoft.EntityFrameworkCore;

namespace EventTestTask.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateAsync(User user, string refreshToken, DateTime expiration,
        CancellationToken cancellationToken)
    {
        var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        if (userEntity is null)
            throw new KeyNotFoundException("User not found.");

        var existingToken = _context.RefreshTokens
            .Where(rt => rt.UserId == userEntity.Id);

        _context.RefreshTokens.RemoveRange(existingToken);

        var tokenEntity = new RefreshToken
        {
            UserId = userEntity.Id,
            User = userEntity,
            Token = refreshToken,
            Expiration = expiration,
        };

        await _context.RefreshTokens.AddAsync(tokenEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return tokenEntity.Token;
    }

    public async Task<(string, DateTime)> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.UserId == userId, cancellationToken);

        if (token is null)
            throw new KeyNotFoundException("User not found.");

        return (token.Token, token.Expiration);
    }

    public async Task<(string, DateTime)> GetByToken(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token is null)
            throw new KeyNotFoundException("Token not found.");

        return (token.Token, token.Expiration);
    }

    public async Task DeleteByTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        await _context.RefreshTokens
            .Where(rt => rt.Token == refreshToken)
            .ExecuteDeleteAsync(cancellationToken);
    }
}