using WebApplication1.Models.DTOs;

namespace WebApplication1.Interface
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterDTO registerDTO);
        Task<string?> LoginAsync(LoginDTO loginDTO);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> SaveVakifbankConsentAsync(int userId, string consentId);
        Task LogoutAsync(string token);
    }
}
