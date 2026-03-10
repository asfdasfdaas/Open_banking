using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class SafeDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 1. If it's a normal number, just read it
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            // 2. If it's a string, intercept and clean it
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();

                // If the bank sent an empty string instead of a number, return 0 instead of crashing!
                if (string.IsNullOrWhiteSpace(str))
                {
                    return 0m;
                }

                // Clean up any weird comma formats (e.g., "1,500.00" -> "1500.00")
                str = str.Replace(",", "");

                if (decimal.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedValue))
                {
                    return parsedValue;
                }
            }

            // 3. Absolute fallback: If it's completely unparseable, default to 0
            return 0m;
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}