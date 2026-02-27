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
            // 1. Scramble the password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // 2. Save to SQL
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

            // 3. Generate a JWT Token
            return CreateToken(user);
        }
        public async Task<bool> UserExists(string username)
        {
            // Checks if any user in the DB already has this username
            return await _db.User.AnyAsync(x => x.UserName == username);
        }
    }
}
