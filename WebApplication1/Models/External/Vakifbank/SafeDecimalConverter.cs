using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class SafeDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // if it's a normal number, just read it
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            // if it's a string, intercept and clean it
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();

                // if the bank sent an empty string instead of a number, return 0 instead of crashing
                if (string.IsNullOrWhiteSpace(str))
                {
                    return 0m;
                }

                // clean up any weird comma formats ("1,500.00" -> "1500.00")
                str = str.Replace(",", "");

                if (decimal.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedValue))
                {
                    return parsedValue;
                }
            }

            // fallback
            return 0m;
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}