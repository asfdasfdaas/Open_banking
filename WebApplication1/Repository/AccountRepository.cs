using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Interface;
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

        public async Task<IEnumerable<AccountTransaction>> GetAccountTransactionsAsync(int accountId, DateTime startDate, DateTime endDate)
        {
            return await _db.AccountTransactions
                            .Where(t => t.AccountListId == accountId
                                     && t.TransactionDate >= startDate
                                     && t.TransactionDate <= endDate)
                            .OrderByDescending(t => t.TransactionDate) // Sort newest first at the database level!
                            .ToListAsync();
        }

        public async Task<IEnumerable<AccountTransaction>> GetBatchTransactionsAsync(List<int> accountIds, DateTime startDate, DateTime endDate)
        {
            return await _db.AccountTransactions
                            .Where(t => accountIds.Contains(t.AccountListId)
                                     && t.TransactionDate >= startDate
                                     && t.TransactionDate <= endDate)
                            .OrderByDescending(t => t.TransactionDate)
                            .ToListAsync();
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

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }

        public async Task<AccountList?> GetAccountForUpdateAsync(string accountNumber, int? userId = null)
        {
            //if has user id look for sender
            //else look for reciever
            if (userId.HasValue)
            {
                return await _db.AccountLists
                    .FromSqlInterpolated($"SELECT * FROM AccountLists WITH (UPDLOCK) WHERE AccountNumber = {accountNumber} AND UserId = {userId.Value}")
                    .FirstOrDefaultAsync();
            }
            return await _db.AccountLists
                .FromSqlInterpolated($"SELECT * FROM AccountLists WITH (UPDLOCK) WHERE AccountNumber = {accountNumber}")
                .FirstOrDefaultAsync();
        }

        public async Task AddTransactionsAsync(IEnumerable<AccountTransaction> transactions)
        {
            await _db.AccountTransactions.AddRangeAsync(transactions);
        }

        public async Task<AccountList?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _db.AccountLists.FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);
        }

        public async Task<decimal> GetTotalOutgoingTodayAsync(int accountId, DateTime startOfToday)
        {
            // The DB math happens here
            return await _db.AccountTransactions
                .Where(t => t.AccountListId == accountId
                         && t.TransactionDate >= startOfToday
                         && t.Amount < 0)
                .SumAsync(t => t.Amount);
        }
    }
}
