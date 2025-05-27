using EventTestTask.Api.DTOs.Registration;

namespace EventTestTask.Api.DTOs.User;

public record UserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email,
    List<RegistrationResponse> Events,
    string PasswordHash
);