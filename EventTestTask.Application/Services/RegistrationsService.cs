using EventTestTask.Core.Entities;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Application.Services;

public class RegistrationsService : IRegistrationsService
{
    private readonly IRegistrationsRepository _registrationsRepository;

    public RegistrationsService(IRegistrationsRepository registrationsRepository)
    {
        _registrationsRepository = registrationsRepository;
    }

    public async Task RegisterForEvent(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        if (!await _registrationsRepository.CheckUserAndEventExistAsync(userId, eventId, cancellationToken))
        {
            throw new KeyNotFoundException("User or event not found");
        }

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
        if (!await _registrationsRepository.CheckUserAndEventExistAsync(userId, eventId, cancellationToken))
        {
            throw new KeyNotFoundException("User or event not found");
        }

        await _registrationsRepository.UnregisterFromEventAsync(eventId, userId, cancellationToken);
    }

    public async Task<PageResult<User>?> GetEventParticipants(PageParams pageParams, Guid eventId,
        CancellationToken cancellationToken)
    {
        if (!await _registrationsRepository.CheckEventExistsAsync(eventId, cancellationToken))
        {
            throw new KeyNotFoundException("Event not found");
        }

        return await _registrationsRepository.GetParticipantsAsync(pageParams, eventId, cancellationToken);
    }

    public async Task<User?> GetEventParticipantById(Guid eventId, Guid userId,
        CancellationToken cancellationToken)
    {
        if (!await _registrationsRepository.CheckUserAndEventExistAsync(userId, eventId, cancellationToken))
        {
            throw new KeyNotFoundException("User or event not found");
        }

        return await _registrationsRepository.GetParticipantByIdAsync(eventId, userId, cancellationToken);
    }
}