using WebApplication1.Models.DTOs;

namespace WebApplication1.Interface
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountListDTO>> GetAllAsync(int userId);
        Task<AccountListDTO?> GetByIdAsync(int id, int userId);
        Task<(AccountListDTO Dto, int Id)> CreateAccountAsync(AccountCreateDTO createDTO, int userId);
        Task<bool> TransferInternalAsync(int userId, TransferDTO transferDto);
        Task<bool> UpdateAccountAsync(int id, int userId, AccountUpdateDTO updateDTO);
        Task<bool> DeleteAccountAsync(int id, int userId);
        Task<IEnumerable<TransactionDTO>?> GetTransactionsAsync(int userId, string accountNumber, DateTime startDate, DateTime endDate);
        Task<DashboardSummaryDto?> GetDashboardSummaryAsync(int userId, string accountNumber, DateTime startDate, DateTime endDate);
    }
}
