using AutoMapper;
using EventTestTask.Core.DTOs.User;
using EventTestTask.Core.Interfaces.Repositories;
using EventTestTask.Core.Interfaces.Services;
using EventTestTask.Core.Models.Pagination;

namespace EventTestTask.Application.Services;

public class RegistartionsService : IRegistrationsService
{
    private readonly IRegistrationsRepository _registrationsRepository;
    private readonly IMapper _mapper;

    public RegistartionsService(IRegistrationsRepository registrationsRepository, IMapper mapper)
    {
        _registrationsRepository = registrationsRepository;
        _mapper = mapper;
    }

    public async Task JoinToEvent(Guid eventId, Guid userId, CancellationToken cancellationToken) =>
        await _registrationsRepository.JoinToEventAsync(eventId, userId, cancellationToken);

    public async Task LeaveFromEvent(Guid eventId, Guid userId, CancellationToken cancellationToken) =>
        await _registrationsRepository.LeaveFromEventAsync(eventId, userId, cancellationToken);

    public async Task<PageResult<UserResponse>?> GetParticipants(PageParams pageaParams, Guid eventId,
        CancellationToken cancellationToken)
    {
        var participants = await _registrationsRepository.GetParticipantsAsync(
            pageaParams, eventId, cancellationToken);
        return _mapper.Map<PageResult<UserResponse>>(participants);
    }

    public async Task<UserResponse?> GetParticipantById(Guid eventId, Guid userId, CancellationToken cancellationToken)
    {
        var participant = await _registrationsRepository.GetParticipantByIdAsync(eventId, userId, cancellationToken);
        return _mapper.Map<UserResponse>(participant);
    }
}