using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Infrastructure.ApplicationContext;
using Microsoft.EntityFrameworkCore;

namespace EventTestTask.Infrastructure.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly AppDbContext _context;

    public UsersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Events)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("User not found");

        return user;
    }

    public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null)
            throw new KeyNotFoundException("User not found");

        return user;
    }

    public async Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        var newUser = new User(
            Guid.NewGuid(),
            user.FirstName,
            user.LastName,
            user.BirthDate,
            user.Email,
            user.PasswordHash,
            user.Role
        );

        await _context.Users.AddAsync(newUser, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserAsync(Guid userId, User user, CancellationToken cancellationToken)
    {
        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.FirstName, user.FirstName)
                .SetProperty(u => u.LastName, user.LastName)
                .SetProperty(u => u.BirthDate, user.BirthDate)
                .SetProperty(u => u.Email, user.Email), cancellationToken);
    }
}