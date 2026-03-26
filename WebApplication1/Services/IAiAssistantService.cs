using WebApplication1.Models.DTOs;

namespace WebApplication1.Services
{
    public interface IAiAssistantService
    {
        Task<string> ChatAsync(string prompt);

        Task<AiSpendingAnalysisResultDTO> AnalyzeSpendingAsync(string accountNumber, DateTime startDate, DateTime endDate);
    }
}

