using EventTestTask.Api.DTOs.Registration;
using EventTestTask.Application.Services;
using EventTestTask.Core.Enums;
using Newtonsoft.Json;

namespace EventTestTask.Api.DTOs.Event;

public record EventResponse(
    Guid Id,
    string Title,
    string Description,
    [property: JsonConverter(typeof(IsoDateTimeConverter))] DateTime StartDate,
    [property: JsonConverter(typeof(IsoDateTimeConverter))] DateTime EndDate,
    string Location,
    EventCategory Category,
    int MaxParticipants,
    byte[] Image,
    List<RegistrationResponse> Participants
);