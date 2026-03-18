using WebApplication1.Models.DTOs;
using WebApplication1.Models.External.Vakifbank;

namespace WebApplication1.Services
{
    public interface IBankIntegrationService
    {
        Task<string> GetBankTokenAsync(string consentId);
        Task<string> GetClientCKeyAsync();
        Task<IEnumerable<AccountListDTO>> GetAccountsFromBankAsync(int userId, string consentId);

        Task<AccountDetailDTO> GetAccountDetailAsync(string accountNumber, string consentId);

        Task<IEnumerable<TransactionDTO>> GetAccountTransactionsAsync(string accountNumber, DateTime startDate, DateTime endDate, string consentId);

        Task<byte[]> GetReceiptPdfAsync(string transactionId, string accountNumber, string consentId);

        Task<decimal> CalculateCurrencyAsync(string sourceCurrency, decimal amount, string targetCurrency);
        Task<DepositProductResponse> GetDepositProductsAsync();
        Task<BranchListResponse> GetBranchListAsync(string? cityCode = null, string? districtCode = null);
        Task<DepositCalculatorResponse> CalculateDepositAsync(DepositCalculatorRequest request);
        Task<ATMListResponse> GetATMListAsync(string? cityCode = null, string? districtCode = null);
    }
}
