using WebApplication1.Models.DTOs;

namespace WebApplication1.Services
{
    public interface IBankIntegrationService
    {
        Task<string> GetBankTokenAsync();
        Task<IEnumerable<AccountListDTO>> GetAccountsFromBankAsync(int userId);
    }
}
