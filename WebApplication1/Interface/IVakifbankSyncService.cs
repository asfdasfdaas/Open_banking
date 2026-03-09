using WebApplication1.Models.DTOs;

namespace WebApplication1.Interface
{
    public interface IVakifbankSyncService
    {
        Task<IEnumerable<AccountListDTO>> SyncAccountsAsync(int userId);
        Task<AccountDetailDTO> GetAndSyncAccountDetailAsync(int userId, string accountNumber);
        Task<IEnumerable<TransactionDTO>> SyncTransactionsAsync(int userId, string accountNumber, DateTime startDate, DateTime endDate);
        Task<byte[]> GetReceiptPdfAsync(int userId, string accountNumber, string transactionId);
    }
}
