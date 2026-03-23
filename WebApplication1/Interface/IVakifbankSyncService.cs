using WebApplication1.Models.DTOs;
using WebApplication1.Models.External.Vakifbank;

namespace WebApplication1.Interface
{
    public interface IVakifbankSyncService
    {
        Task<IEnumerable<AccountListDTO>> SyncAccountsAsync(int userId);
        Task<decimal> CalculateCurrencyAsync(string sourceCurrency, decimal amount, string targetCurrency);
        Task<AccountDetailDTO> GetAndSyncAccountDetailAsync(int userId, string accountNumber);
        Task<IEnumerable<TransactionDTO>> SyncTransactionsAsync(int userId, string accountNumber, DateTime startDate, DateTime endDate);
        Task<byte[]> GetReceiptPdfAsync(int userId, string accountNumber, string transactionId);
        Task<DepositProductResponse> GetDepositProductsAsync();
        Task<BranchListResponse> GetBranchListAsync(string? cityCode = null, string? districtCode = null);
        Task<DepositCalculatorResponse> CalculateDepositAsync(DepositCalculatorRequest request);
        Task<ATMListResponse> GetATMListAsync(string? cityCode = null, string? districtCode = null);
        Task<CityListResponse> GetCityListAsync();
        Task<DistrictListResponse> GetDistrictListAsync(string cityCode);
        Task<NeighborhoodListResponse> GetNeighborhoodListAsync(string districtCode);
    }
}
