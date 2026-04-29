using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Interface;
using WebApplication1.Models;

namespace WebApplication1.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDBContext _db;

        public AuthRepository(ApplicationDBContext db)
        {
            _db = db;
        }
        public async Task<User?> Register(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            _db.User.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _db.User.FirstOrDefaultAsync(x => x.UserName == username);
        }
        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _db.User.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return false;
            }
            _db.User.Remove(user);
            return await _db.SaveChangesAsync() > 0;
        }
        public async Task<bool> UserExists(string username)
        {
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

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _db.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
            if (rt != null)
            {
                rt.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId)
        {
            var tokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();
            tokens.ForEach(t => t.IsRevoked = true);
            await _db.SaveChangesAsync();
        }
    }
}
