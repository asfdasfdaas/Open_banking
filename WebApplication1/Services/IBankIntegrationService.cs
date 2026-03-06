using WebApplication1.Models.DTOs;

namespace WebApplication1.Services
{
    public interface IBankIntegrationService
    {
        Task<string> GetBankTokenAsync(string consentId);
        Task<IEnumerable<AccountListDTO>> GetAccountsFromBankAsync(int userId, string consentId);

        Task<AccountDetailDTO> GetAccountDetailAsync(string accountNumber, string consentId);

        Task<IEnumerable<TransactionDTO>> GetAccountTransactionsAsync(string accountNumber, DateTime startDate, DateTime endDate, string consentId);

        Task<byte[]> GetReceiptPdfAsync(string transactionId, string accountNumber, string consentId);
    }
}
