using EventTestTask.Application.Services;
using Newtonsoft.Json;

namespace EventTestTask.Api.DTOs.User;

public record RegisterUser(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    [property: JsonConverter(typeof(IsoDateTimeConverter))] DateTime BirthDate
);