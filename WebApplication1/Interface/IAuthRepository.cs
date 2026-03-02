using WebApplication1.Models;

namespace WebApplication1.Interface
{
    public interface IAuthRepository
    {
        Task<User?> Register(User user, string password);
        Task<string?> Login(string username, string password); // Returns a Token string
        Task<bool> DeleteUser(int userId);
        Task<bool> UserExists(string username);
        Task<bool> EmailExists(string email);
    }
}
