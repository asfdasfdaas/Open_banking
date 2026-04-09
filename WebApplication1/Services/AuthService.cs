using WebApplication1.Interface;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApplication1.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository repo, IMemoryCache cache, IConfiguration config)
        {
            _repo = repo;
            _cache = cache;
            _config = config;
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
            var user = await _repo.GetByUsernameAsync(loginDTO.Username.ToLower());
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
            {
                return null;
            }

            return CreateToken(user);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await _repo.DeleteUser(userId);
        }

        public async Task<bool> SaveVakifbankConsentAsync(int userId, string consentId)
        {
            return await _repo.SaveVakifbankConsentAsync(userId, consentId);
        }

        public async Task LogoutAsync(string token)
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

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                var consentId = await _repo.GetVakifbankConsentIdAsync(userId);
                if (!string.IsNullOrWhiteSpace(consentId))
                {
                    var cacheKey = $"VakifbankToken_{consentId}";
                    _cache.Remove(cacheKey);
                }
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _config.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(15),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
