using EventTestTask.Core.Enums;

namespace EventTestTask.Api.DTOs.Event;

public record EventRequest(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    EventCategory Category,
    int MaxParticipants,
    byte[]? Image
);