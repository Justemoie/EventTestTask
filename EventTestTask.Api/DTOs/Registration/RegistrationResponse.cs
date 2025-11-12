using EventTestTask.Application.Services;
using Newtonsoft.Json;

namespace EventTestTask.Api.DTOs.Registration;

public record RegistrationResponse(
    Guid Id,
    Guid UserId,
    Guid EventId,
    [property: JsonConverter(typeof(IsoDateTimeConverter))] DateTime RegistrationDate
);