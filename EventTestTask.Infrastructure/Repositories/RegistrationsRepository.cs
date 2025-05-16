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

    public async Task JoinToEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var @event = await _context.Events
            .FindAsync(eventId, cancellationToken);
        if (@event is null)
            throw new KeyNotFoundException("Event not found");

        var user = await _context.Users
            .FindAsync(userId, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException("User not found");

        var newRegistration = new Registration(
            Guid.NewGuid(),
            userId,
            eventId,
            DateTime.UtcNow
        );

        await _context.Registrations.AddAsync(newRegistration, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LeaveFromEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations
            .SingleOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId, cancellationToken);

        if (registration is null)
            throw new KeyNotFoundException("Registration not found");

        _context.Registrations.Remove(registration);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PageResult<User>?> GetParticipantsAsync(PageParams pageaParams, Guid eventId,
        CancellationToken cancellationToken)
    {
        var query = _context.Registrations.AsQueryable();

        var participants = await query
            .AsNoTracking()
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