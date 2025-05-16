using EventTestTask.Core.DTOs.Registration;

namespace EventTestTask.Core.DTOs.User;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email,
    List<RegistrationResponse> Events
);