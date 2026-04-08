using System.Linq;
using WebApplication1.Services;
using WebApplication1.Interface;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Services.Providers
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly IGeminiIntegrationService _geminiService;
        private readonly IAccountRepository _accountRepository;

        public AiAssistantService(IGeminiIntegrationService geminiService, IAccountRepository accountRepository)
        {
            _geminiService = geminiService;
            _accountRepository = accountRepository;
        }

        public async Task<string> ChatAsync(string prompt)
        {
            return await _geminiService.GetChatResponseAsync(prompt);
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

            // Turn transactions into a simple text ledger for the LLM to read.
            var txData = string.Join("\n", transactions.Select(t =>
                $"{t.TransactionDate:yyyy-MM-dd} | {t.Description} | {t.Amount} TL"));

            var systemContext =
                "You are an elite financial advisor. You will be provided with a user's transaction history. " +
                "Analyze their spending habits, identify patterns, and provide concise, highly actionable bullet points of financial advice. " +
                "Keep your tone professional, encouraging, and brief. Format your answer clearly.";

            var userPrompt = $"Here are the transactions:\n{txData}\n\nPlease provide your analysis.";

            var responseText = await _geminiService.GetChatResponseAsync(userPrompt, systemContext);
            return new AiSpendingAnalysisResultDTO { AccountFound = true, Advice = responseText };
        }
    }
}

