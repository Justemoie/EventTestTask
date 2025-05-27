namespace EventTestTask.Api.DTOs.User;

public record RegisterUser(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    DateTime BirthDate
);