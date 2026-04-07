using WebApplication1.Interface;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;

namespace WebApplication1.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IMemoryCache _cache;

        public AuthService(IAuthRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
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

        public Task LogoutAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var expiresAt = jwtToken.ValidTo;
            var timeRemaining = expiresAt - DateTime.UtcNow;

            // Cache the token for its remaining lifespan
            if (timeRemaining > TimeSpan.Zero)
            {
                _cache.Set(token, "Revoked", timeRemaining);
            }

            return Task.CompletedTask;
        }
    }
}
