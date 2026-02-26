using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDBContext _db;
        public AccountRepository(ApplicationDBContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<AccountList>> GetAllAsync()
        {
            return await _db.AccountLists.ToListAsync();
        }

        public async Task<AccountList?> GetByIdAsync(int id)
        {
            return await _db.AccountLists.FindAsync(id);
        }

        public async Task CreateAsync(AccountList account)
        {
            await _db.AccountLists.AddAsync(account);
            await SaveAsync();
        }
        public async Task DeleteAsync(AccountList account)
        {
            _db.AccountLists.Remove(account);
            await SaveAsync();
        }

        public async Task UpdateAsync(AccountList account)
        {
            _db.AccountLists.Update(account);
            await SaveAsync();
        }


        public async Task<bool> SaveAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
