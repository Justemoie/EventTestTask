using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventTestTask.Application.Services;

public class IsoDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (DateTime.TryParseExact(value, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
            return date;

        if (DateTime.TryParse(value, out date))
            return date;

        throw new JsonException($"Invalid date format: {value}. Expected: yyyy-MM-dd");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}