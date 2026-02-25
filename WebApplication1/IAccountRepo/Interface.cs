using WebApplication1.Models.DTOs;

namespace WebApplication1.Interfaces
{
    public interface IAccountRepo
    {
        Task<List<AccountListDTO>> GetAllAccountsAsync();
    }
}
