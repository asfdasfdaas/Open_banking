using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class FlexibleIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // if the bank correctly sends a number 
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            // if the bank sends a string 
            if (reader.TokenType == JsonTokenType.String)
            {
                string ?val = reader.GetString();

                // turn empty strings into null
                if (string.IsNullOrWhiteSpace(val))
                    return null;

                // turn string numbers into integers
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
