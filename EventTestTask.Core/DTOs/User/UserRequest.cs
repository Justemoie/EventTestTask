namespace EventTestTask.Core.DTOs.User;

public record UserRequest(
    string FirstName,
    string LastName,
    DateTime BirthDate,
    string Email
);