using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Vakifbank
{
    public class SingleOrArrayConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // if the bank sends a normal array [ {tx1}, {tx2} ]
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                return JsonSerializer.Deserialize<List<T>>(ref reader, options) ?? new List<T>();
            }
            // if the bank sends a single object {tx1} because there is only 1 transaction
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                var singleItem = JsonSerializer.Deserialize<T>(ref reader, options);
                return singleItem != null ? new List<T> { singleItem } : new List<T>();
            }
            // If it's empty or null
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return new List<T>();
            }

            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
