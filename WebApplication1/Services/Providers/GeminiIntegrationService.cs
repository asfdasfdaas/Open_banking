using System.Text;
using System.Text.Json;
using WebApplication1.Models.External.Gemini;

namespace WebApplication1.Services.Providers
{
    public class GeminiIntegrationService : IGeminiIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiIntegrationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetChatResponseAsync(string userPrompt, string? customContext = null)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("Gemini API Key is missing!");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={apiKey}";

            // Use the custom context if provided, otherwise default to the general chat persona
            var defaultContext = "You are a professional, highly intelligent financial assistant for a modern open banking platform. Keep answers concise. This is development version";
            var activeContext = customContext ?? defaultContext;

            var requestPayload = new GeminiRequest
            {
                SystemInstruction = new GeminiContent
                {
                    Parts = new List<GeminiPart> { new GeminiPart { Text = activeContext } }
                },
                Contents = new List<GeminiContent>
                {
                    new GeminiContent
                    {
                        Parts = new List<GeminiPart> { new GeminiPart { Text = userPrompt } }
                    }
                }
            };

            var jsonString = JsonSerializer.Serialize(requestPayload);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            // 4. Send it!
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API Failed: {error}");
            }

            // 5. Parse the deeply nested response to grab just the text
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

            // Navigate safely through the JSON to find the actual text answer
            var answerText = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            return answerText ?? "Sorry, I couldn't generate a response.";
        }
    }
}