using WebApplication1.Models;

namespace WebApplication1.Interface
{
    public interface IAuthRepository
    {
        Task<User?> Register(User user, string password);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> DeleteUser(int userId);
        Task<bool> UserExists(string username);
        Task<bool> EmailExists(string email);
        Task<string?> GetVakifbankConsentIdAsync(int userId);
        Task<bool> SaveVakifbankConsentAsync(int userId, string consentId);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserRefreshTokensAsync(int userId);
    }
}
