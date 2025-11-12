using System.Text.Json.Serialization;
using EventTestTask.Application.Services;
using EventTestTask.Core.Enums;

namespace EventTestTask.Api.DTOs.Event;

public record EventRequest(
    string Title,
    string Description,
    [property: Newtonsoft.Json.JsonConverter(typeof(IsoDateTimeConverter))] DateTime StartDate,
    [property: Newtonsoft.Json.JsonConverter(typeof(IsoDateTimeConverter))] DateTime EndDate,
    string Location,
    [Newtonsoft.Json.JsonConverter(typeof(JsonStringEnumConverter))] EventCategory Category,
    int MaxParticipants,
    byte[]? Image
);