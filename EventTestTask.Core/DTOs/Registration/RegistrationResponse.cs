namespace EventTestTask.Core.DTOs.Registration;

public record RegistrationResponse(
    Guid Id,
    Guid UserId,
    Guid EventId,
    DateTime RegistrationDate
);