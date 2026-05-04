using System.Text;
using System.Text.Json;
using WebApplication1.Models.DTOs;
using WebApplication1.Interface;

namespace WebApplication1.Services.Providers
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly IAccountRepository _accountRepository;

        public AiAssistantService(HttpClient httpClient, IAccountRepository accountRepository)
        {
            _httpClient = httpClient;
            _accountRepository = accountRepository;
        }

        public async Task<string> ChatAsync(string prompt)
        {
            var systemContext = "You are a professional, highly intelligent financial assistant for a modern open banking platform." +
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
                "Keep answers concise. Here is user text:";

            return await GetOllamaResponseAsync(prompt, systemContext);
        }

        public async Task<AiSpendingAnalysisResultDTO> AnalyzeSpendingAsync(string accountNumber, DateTime startDate, DateTime endDate)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (account == null)
                return new AiSpendingAnalysisResultDTO { AccountFound = false, Advice = string.Empty };

            var transactions = await _accountRepository.GetAccountTransactionsAsync(account.Id, startDate, endDate);
            if (!transactions.Any())
            {
                return new AiSpendingAnalysisResultDTO
                {
                    AccountFound = true,
                    Advice = "No transactions found in this date range to analyze."
                };
            }

            var txData = string.Join("\n", transactions.Select(t =>
                $"{t.TransactionDate:yyyy-MM-dd} | {t.Description} | {t.Amount} TL"));

            var systemContext =
                "You are an elite financial advisor. You will be provided with a user's transaction history. " +
                "Analyze their spending habits, identify patterns, and provide concise, highly actionable bullet points of financial advice. " +
                "Keep your tone professional, encouraging, and brief. Format your answer clearly.";

            var userPrompt = $"Here are the transactions:\n{txData}\n\nPlease provide your analysis.";

            var responseText = await GetOllamaResponseAsync(userPrompt, systemContext);

            return new AiSpendingAnalysisResultDTO { AccountFound = true, Advice = responseText };
        }

        // --- Private Helper Method to handle the Local AI API Call ---
        private async Task<string> GetOllamaResponseAsync(string userPrompt, string systemContext)
        {
            // JSON payload Ollama expects
            var requestPayload = new
            {
                model = "phi3",
                stream = false,
                messages = new[]
                {
                    new { role = "system", content = systemContext },
                    new { role = "user", content = userPrompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");

            
            var response = await _httpClient.PostAsync("/api/chat", content);
            response.EnsureSuccessStatusCode();

            
            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            
            var aiText = document.RootElement
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();

            return aiText ?? "I'm sorry, I could not generate an analysis at this time.";
        }
    }
}