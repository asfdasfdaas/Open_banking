using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<AccountList>> GetAllAsync();
        Task<AccountList?> GetByIdAsync(int id);
        Task CreateAsync(AccountList account);
        Task UpdateAsync(AccountList account);
        Task DeleteAsync(AccountList account);
        Task<bool> SaveAsync(); 
    }
}

