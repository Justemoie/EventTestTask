using EventTestTask.Core.DTOs.Registration;
using EventTestTask.Core.Enums;

namespace EventTestTask.Core.DTOs.Event;

public record EventResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    EventCategory Category,
    int MaxParticipants,
    byte[] Image,
    List<RegistrationResponse> Participants
);