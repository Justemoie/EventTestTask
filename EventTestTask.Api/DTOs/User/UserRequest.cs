namespace EventTestTask.Api.DTOs.User;

public record UserRequest(
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email,
    string PasswordHash
);