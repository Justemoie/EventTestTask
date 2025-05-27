using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Application.Services;

public class RegistrationsService : IRegistrationsService
{
    private readonly IRegistrationsRepository _registrationsRepository;
    private readonly IEventsRepository _eventsRepository;
    private readonly IUsersRepository _usersRepository;

    public RegistrationsService(IRegistrationsRepository registrationsRepository, IEventsRepository eventsRepository,
        IUsersRepository usersRepository)
    {
        _registrationsRepository = registrationsRepository;
        _eventsRepository = eventsRepository;
        _usersRepository = usersRepository;
    }

    public async Task RegisterForEvent(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        await CheckUserAndEventExists(userId, eventId, cancellationToken);
        var newRegistration = new Registration(
            Guid.NewGuid(),
            userId,
            eventId,
            DateTime.UtcNow
        );

        await _registrationsRepository.RegisterForEventAsync(eventId, userId, newRegistration, cancellationToken);
    }

    public async Task UnregisterFromEvent(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        await CheckUserAndEventExists(userId, eventId, cancellationToken);
        await _registrationsRepository.UnregisterFromEventAsync(eventId, userId, cancellationToken);
    }

    public async Task<PageResult<User>?> GetEventParticipants(PageParams pageParams, Guid eventId,
        CancellationToken cancellationToken)
    {
        var @event = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        if (@event is null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        return await _registrationsRepository.GetParticipantsAsync(pageParams, eventId, cancellationToken);
    }

    public async Task<User?> GetEventParticipantById(Guid eventId, Guid userId,
        CancellationToken cancellationToken)
    {
        await CheckUserAndEventExists(userId, eventId, cancellationToken);
        return await _registrationsRepository.GetParticipantByIdAsync(eventId, userId, cancellationToken);
    }

    private async Task CheckUserAndEventExists(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var @event = await _eventsRepository.GetEventByIdAsync(eventId, cancellationToken);
        var user = await _usersRepository.GetUserByIdAsync(userId, cancellationToken);
        if (@event is null || user is null)
        {
            throw new KeyNotFoundException("User or event not found");
        }
    }
}