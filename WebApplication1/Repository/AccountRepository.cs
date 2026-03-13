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
        public async Task<IEnumerable<AccountList>> GetUserAccountsAsync(int userId)
        {   
            return await _db.AccountLists
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task<AccountList?> GetByIdAsync(int id, int userId)
        {
            return await _db.AccountLists.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
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
        public async Task<List<string>> GetExistingTransactionIdsAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            return await _db.AccountTransactions
                            .Where(t => t.AccountListId == accountId
                                && t.TransactionDate >= startDate
                                && t.TransactionDate <= endDate
                            )
                            .Select(t => t.TransactionId)
                            .ToListAsync();
        }

        public async Task SaveTransactionsAsync(IEnumerable<AccountTransaction> transactions)
        {
            await _db.AccountTransactions.AddRangeAsync(transactions);
            await SaveAsync();
        }
    }
}
