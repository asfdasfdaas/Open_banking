using WebApplication1.Interface;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;

        public AuthService(IAuthRepository repo)
        {
            _repo = repo;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO registerDTO)
        {
            if (await _repo.UserExists(registerDTO.Username.ToLower()))
            {
                return (false, "Username already exists.");
            }
            if (await _repo.EmailExists(registerDTO.Email.ToLower()))
            {
                return (false, "Email already exists.");
            }
            
            var userToCreate = new User
            {
                UserName = registerDTO.Username.ToLower(),
                Email = registerDTO.Email.ToLower()
            };
            
            var createdUser = await _repo.Register(userToCreate, registerDTO.Password);
            if (createdUser == null)
            {
                return (false, "An error occurred while creating the user.");
            }
            
            return (true, "User registered successfully.");
        }

        public async Task<string?> LoginAsync(LoginDTO loginDTO)
        {
            return await _repo.Login(loginDTO.Username.ToLower(), loginDTO.Password);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await _repo.DeleteUser(userId);
        }

        public async Task<bool> SaveVakifbankConsentAsync(int userId, string consentId)
        {
            return await _repo.SaveVakifbankConsentAsync(userId, consentId);
        }
    }
}
