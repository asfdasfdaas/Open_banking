using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> TransferMoneyInternalAsync(int userId, TransferDTO transferDto)
        {
            
            // If the app crashes at any point, the database will automatically roll back to its original state.
            await using var dbTransaction = await _db.Database.BeginTransactionAsync();

            try
            {
                // fetch and validate sender
                var senderAccount = await _db.AccountLists
                    .FirstOrDefaultAsync(a => a.AccountNumber == transferDto.SenderAccountNumber && a.UserId == userId);

                if (senderAccount == null || senderAccount.ProviderName != "Internal")
                    throw new Exception("Invalid sender account. Ensure it is an internal account that belongs to you.");

                // fetch and validate receiver
                var receiverAccount = await _db.AccountLists
                    .FirstOrDefaultAsync(a => a.AccountNumber == transferDto.ReceiverAccountNumber);

                if (receiverAccount == null || receiverAccount.ProviderName != "Internal")
                    throw new Exception("Invalid receiver account. Destination must be an active internal account.");

                // Business rules validation
                if (transferDto.Amount <= 0)
                    throw new Exception("Transfer amount must be greater than zero.");

                if (senderAccount.Id == receiverAccount.Id)
                    throw new Exception("You cannot transfer money to the same account.");

                if (senderAccount.CurrencyCode != receiverAccount.CurrencyCode)
                    throw new Exception($"Currency mismatch. Cannot transfer {senderAccount.CurrencyCode} to a {receiverAccount.CurrencyCode} account without FX conversion.");

                if (senderAccount.RemainingBalance < transferDto.Amount)
                    throw new Exception("Insufficient funds.");

                senderAccount.Balance -= transferDto.Amount;
                senderAccount.RemainingBalance -= transferDto.Amount;

                receiverAccount.Balance += transferDto.Amount;
                receiverAccount.RemainingBalance += transferDto.Amount;

                var timestamp = DateTime.UtcNow;

                // The negative transaction for the sender
                var senderTx = new AccountTransaction
                {
                    AccountListId = senderAccount.Id,
                    TransactionId = Guid.NewGuid().ToString(), // Generate a unique receipt ID
                    TransactionName = "Outgoing Transfer",
                    Description = string.IsNullOrWhiteSpace(transferDto.Description) ? $"Transfer to {receiverAccount.AccountNumber}" : transferDto.Description,
                    TransactionType = "Outgoing",
                    Amount = -transferDto.Amount,
                    Balance = senderAccount.Balance, // The snapshot of their balance after the transfer
                    TransactionDate = timestamp,
                    CurrencyCode = senderAccount.CurrencyCode
                };

                // The positive transaction for the receiver
                var receiverTx = new AccountTransaction
                {
                    AccountListId = receiverAccount.Id,
                    TransactionId = Guid.NewGuid().ToString(),
                    TransactionName = "Incoming Transfer",
                    Description = string.IsNullOrWhiteSpace(transferDto.Description) ? $"Transfer from {senderAccount.AccountNumber}" : transferDto.Description,
                    TransactionType = "Incoming",
                    Amount = transferDto.Amount,
                    Balance = receiverAccount.Balance,
                    TransactionDate = timestamp,
                    CurrencyCode = receiverAccount.CurrencyCode
                };

                // Queue the new transactions to be saved
                await _db.AccountTransactions.AddRangeAsync(senderTx, receiverTx);

                // 7. SAVE & COMMIT (Durability)
                await SaveAsync();
                await dbTransaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                // ROLLBACK
                // If ANY step above fails (e.g., insufficient funds, or the DB crashes), 
                // this line safely reverts the Sender and Receiver balances back to what they were.
                await dbTransaction.RollbackAsync();

                throw; // Re-throw the error so Controller can read the message (e.g., "Insufficient funds")
            }
        }
        public async Task<AccountList?> GetByAccountNumberAsync(string accountNumber)
        {
            // Searches the database for the matching string account number
            return await _db.AccountLists.FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);
        }
    }
}
