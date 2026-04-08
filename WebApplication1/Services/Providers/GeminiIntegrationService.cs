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

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}"; //gemini-2.5-flash ya da gemini-3-flash-preview

            // Use the custom context if provided, otherwise default to the general chat persona
            var defaultContext = "You are a professional, highly intelligent financial assistant for a modern open banking platform." +
                "an app to make managing multiple accounts and finances easier." +
                "You do not have the capacity to do any action other then just chatting. " +
                "You do not have access to any user/account data. Just this context promt and user text. " +
                "Current platform capabilities:" +
                "The app has internal accounts and vakıfbank accounts if the user connected the vakifbank account in the dashboard page. " +
                "home page has live Exchange rates for: Gold, silver, american dolar, japanse yen, british sterling, euro " +
                "and a vakıfbank brach locator where you can choose city and district, and it will show you vakıfbank accounts there. " +
                "Dashboard has users accounts and deposit calculator. " +
                "Each account has an account detail page that has info like the transaction past and useful graphs and features for chosen date range like: net flow number, flow ratio pie chart graph, balance trend graph. " +
                "Account details page also has an ai advisor that looks at your transactions from the chosen date range and gives advice base on that. " +
                "Keep answers concise.";
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