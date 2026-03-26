using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<AccountList>> GetAllAsync();
        Task<IEnumerable<AccountList>> GetUserAccountsAsync(int userId);
        Task<AccountList?> GetByIdAsync(int id, int userId);
        Task CreateAsync(AccountList account);
        Task UpdateAsync(AccountList account);
        Task DeleteAsync(AccountList account);
        Task<IEnumerable<AccountTransaction>> GetAccountTransactionsAsync(int accountId, DateTime startDate, DateTime endDate);
        Task<bool> SaveAsync();
        Task<List<string>> GetExistingTransactionIdsAsync(int accountId, DateTime startDate, DateTime endDate);
        Task SaveTransactionsAsync(IEnumerable<AccountTransaction> transactions);
        Task<bool> TransferMoneyInternalAsync(int userId, TransferDTO transferDto);
        Task<AccountList?> GetByAccountNumberAsync(string accountNumber);
    }
}

