using EventTestTask.Core.DTOs.Event;

namespace EventTestTask.Core.DTOs.User;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email,
    List<EventResponse> Events
);