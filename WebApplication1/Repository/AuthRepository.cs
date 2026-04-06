using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Interface;
using WebApplication1.Models;

namespace WebApplication1.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDBContext _db;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public AuthRepository(ApplicationDBContext db, IConfiguration config, IMemoryCache cache)
        {
            _db = db;
            _config = config;
            _cache = cache;
        }
        private string CreateToken(User user)
        {
            // Identify what information (Claims) we want to "bake" into the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // Get the Secret Key from appsettings.json
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _config.GetValue<string>("AppSettings:Token")!));

            // Create the Signing Credentials (The Digital Stamp)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Create the Token Descriptor (The "Specs" of the token)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1), // Token lasts 24 hours
                SigningCredentials = creds
            };

            // Generate and Return the string
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        public async Task<User?> Register(User user, string password)
        {
            // Scramble the password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Save to SQL
            _db.User.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
        public async Task<string?> Login(string username, string password)
        {
            var user = await _db.User.FirstOrDefaultAsync(x => x.UserName == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Bad username or password
            }

            if (!string.IsNullOrEmpty(user.VakifbankConsentId))
            {
                string cacheKey = $"VakifbankToken_{user.VakifbankConsentId}";
                _cache.Remove(cacheKey);
            }

            // Generate a JWT Token
            return CreateToken(user);
        }
        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _db.User.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return false; // Bad username or password
            }
            _db.User.Remove(user);
            return await _db.SaveChangesAsync() > 0;
        }
        public async Task<bool> UserExists(string username)
        {
            // Checks if any user in the DB already has this username
            return await _db.User.AnyAsync(x => x.UserName == username.ToLower());
        }
        public async Task<bool> EmailExists(string email)
        {
            return await _db.User.AnyAsync(x => x.Email == email.ToLower());
        }

        public async Task<string?> GetVakifbankConsentIdAsync(int userId)
        {
            var user = await _db.User.FirstOrDefaultAsync(u => u.Id == userId);
            return user?.VakifbankConsentId;
        }

        public async Task<bool> SaveVakifbankConsentAsync(int userId, string consentId)
        {
            var user = await _db.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            user.VakifbankConsentId = consentId;
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
