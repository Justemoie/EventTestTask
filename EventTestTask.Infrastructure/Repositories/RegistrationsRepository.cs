using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Models.Pagination;
using EventTestTask.Infrastructure.ApplicationContext;
using EventTestTask.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace EventTestTask.Infrastructure.Repositories;

public class RegistrationsRepository : IRegistrationsRepository
{
    private readonly AppDbContext _context;

    public RegistrationsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegisterForEventAsync(Guid eventId, Guid userId, Registration newRegistration,
        CancellationToken cancellationToken)
    {
        await _context.Registrations.AddAsync(newRegistration, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnregisterFromEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .SingleOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId, cancellationToken);

        _context.Registrations.Remove(registration!);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PageResult<User>?> GetParticipantsAsync(PageParams pageaParams, Guid eventId,
        CancellationToken cancellationToken)
    {
        var participants = await _context.Registrations
            .AsNoTracking()
            .AsQueryable()
            .Where(r => r.EventId == eventId)
            .Select(r => r.User)
            .ToPage(pageaParams, cancellationToken);
        return participants;
    }

    public async Task<User?> GetParticipantByIdAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var participant = await _context.Registrations
            .AsNoTracking()
            .Where(r => r.EventId == eventId && r.UserId == userId)
            .Select(r => r.User)
            .SingleOrDefaultAsync(cancellationToken);
        return participant;
    }
}