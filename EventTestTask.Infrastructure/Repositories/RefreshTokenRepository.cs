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

    public async Task CreateAsync(User? user, RefreshToken newToken, CancellationToken cancellationToken)
    {
        var existingToken = _context.RefreshTokens
            .Where(rt => rt.UserId == user!.Id);

        _context.RefreshTokens.RemoveRange(existingToken);

        await _context.RefreshTokens.AddAsync(newToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(string, DateTime)> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.UserId == userId, cancellationToken);
        return (token!.Token, token.Expiration);
    }

    public async Task<(string, DateTime)> GetByToken(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
        return (token!.Token, token.Expiration);
    }

    public async Task DeleteByTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        await _context.RefreshTokens
            .Where(rt => rt.Token == refreshToken)
            .ExecuteDeleteAsync(cancellationToken);
    }
}