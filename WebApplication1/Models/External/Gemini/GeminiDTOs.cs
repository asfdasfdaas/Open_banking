using System.Text.Json.Serialization;

namespace WebApplication1.Models.External.Gemini
{
    // --- REQUEST OBJECTS  ---
    public class GeminiRequest
    {
        // Add the new System Instruction property (make it nullable just in case)
        [JsonPropertyName("systemInstruction")]
        public GeminiContent? SystemInstruction { get; set; }

        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();
    }

    public class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();

        [JsonPropertyName("role")]
        public string Role { get; set; } = "user"; // Can be 'user' or 'model'
    }

    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    // --- RESPONSE OBJECTS  ---
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate> Candidates { get; set; } = new();
    }

    public class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent Content { get; set; } = new();
    }
}