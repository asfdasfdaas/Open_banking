using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class FlexibleIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // If the bank correctly sends a number (e.g., 1)
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            // If the bank mistakenly sends a string (e.g., "1" or "")
            if (reader.TokenType == JsonTokenType.String)
            {
                string ?val = reader.GetString();

                // Turn empty strings into null to prevent crashes
                if (string.IsNullOrWhiteSpace(val))
                    return null;

                // Turn string numbers into actual integers
                if (int.TryParse(val, out int result))
                    return result;
            }

            // Default fallback
            return null;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
