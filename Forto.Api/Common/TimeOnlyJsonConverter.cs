using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Forto.Api.Common
{
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private const string Format = "HH:mm";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // case 1: string "08:00"
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();

                if (string.IsNullOrWhiteSpace(value))
                    throw new JsonException("TimeOnly value is empty. Use 'HH:mm' e.g. 08:00.");

                if (TimeOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                    return t;

                if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out t))
                    return t;

                throw new JsonException($"Invalid time format. Use '{Format}' e.g. 08:00.");
            }

            // case 2: object { "hour": 8, "minute": 0 }
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                int? hour = null;
                int? minute = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    var prop = reader.GetString();
                    reader.Read();

                    if (string.Equals(prop, "hour", StringComparison.OrdinalIgnoreCase))
                        hour = reader.GetInt32();
                    else if (string.Equals(prop, "minute", StringComparison.OrdinalIgnoreCase))
                        minute = reader.GetInt32();
                    else
                        reader.Skip();
                }

                if (hour is null || minute is null)
                    throw new JsonException("TimeOnly object must contain 'hour' and 'minute'.");

                if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
                    throw new JsonException("Invalid hour/minute values.");

                return new TimeOnly(hour.Value, minute.Value);
            }

            throw new JsonException("Invalid TimeOnly JSON. Use 'HH:mm' string or {hour, minute} object.");
        }


        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
