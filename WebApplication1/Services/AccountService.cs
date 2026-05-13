using System.Security.Principal;
using WebApplication1.Interface;
using WebApplication1.Mapper;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;

        public AccountService(IAccountRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<AccountListDTO>> GetAllAsync(int userId)
        {
            var accounts = await _repo.GetUserAccountsAsync(userId);
            return accounts.Select(s => s.ToAccountDto()).ToList();
        }

        public async Task<AccountListDTO?> GetByIdAsync(int id, int userId)
        {
            var account = await _repo.GetByIdAsync(id, userId);
            if (account == null)
            {
                return null;
            }
            return account.ToAccountDto();
        }

        public async Task<(AccountListDTO Dto, int Id)> CreateAccountAsync(AccountCreateDTO createDTO, int userId)
        {
            var random = new Random();

            string newAccountNumber = string.Empty;
            for (int i = 0; i < 16; i++) newAccountNumber += random.Next(0, 10);

            string ibanDigits = string.Empty;
            for (int i = 0; i < 24; i++) ibanDigits += random.Next(0, 10);
            string newIban = $"TR{ibanDigits}";

            var newAccount = new Models.AccountList
            {
                UserId = userId,
                AccountNumber = newAccountNumber,
                IBAN = newIban,
                Balance = createDTO.Balance,
                RemainingBalance = createDTO.Balance, // Remaining balance starts equal to the initial deposit
                CurrencyCode = createDTO.CurrencyCode,
                AccountType = 1,           // Default to 1
                AccountStatus = "A",       // "A" for Active
                LastTransactionDate = DateTime.UtcNow,
                ProviderName = "Internal"
            };
            newAccount.UserId = userId;

            await _repo.CreateAsync(newAccount);

            return (newAccount.ToAccountDto(), newAccount.Id);
        }

        public async Task<bool> TransferInternalAsync(int userId, TransferDTO transferDto)
        {
            // if the app crashes at any point the database will automatically roll back to its original state
            await using var dbTransaction = await _repo.BeginTransactionAsync();

            try
            {
                decimal dailyTransferLimit = 5000.00m;
                var startOfToday = DateTime.UtcNow.Date;

                // fetch and validate sender
                var senderAccount = await _repo.GetAccountForUpdateAsync(transferDto.SenderAccountNumber, userId);

                if (senderAccount == null || senderAccount.ProviderName != "Internal")
                    throw new Exception("Invalid sender account. Ensure it is an internal account that belongs to you.");

                // fetch and validate receiver
                var receiverAccount = await _repo.GetAccountForUpdateAsync(transferDto.ReceiverAccountNumber);

                if (receiverAccount == null || receiverAccount.ProviderName != "Internal")
                    throw new Exception("Invalid receiver account. Destination must be an active internal account.");

                // business rules validation
                var outgoingTransfersToday = await _repo.GetTotalOutgoingTodayAsync(senderAccount.Id, startOfToday);
                Console.WriteLine($"Total outgoing transfers today for account {senderAccount.AccountNumber}: {outgoingTransfersToday:C}");

                decimal totalSpentToday = Math.Abs(outgoingTransfersToday);

                if (totalSpentToday + transferDto.Amount > dailyTransferLimit)
                {
                    decimal remainingLimit = dailyTransferLimit - totalSpentToday;
                    throw new Exception($"Daily transfer limit exceeded. You can only transfer up to {remainingLimit:C} more today.");
                }

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

                // the negative transaction for the sender
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

                // the positive transaction for the receiver
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

                // queue the new transactions to be saved
                await _repo.AddTransactionsAsync(new[] { senderTx, receiverTx });

                // save & commit
                await _repo.SaveAsync();
                await dbTransaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {

                await dbTransaction.RollbackAsync();

                throw;
            }
        }

        public async Task<bool> UpdateAccountAsync(int id, int userId, AccountUpdateDTO updateDTO)
        {
            var account = await _repo.GetByIdAsync(id, userId);
            if (account == null)
            {
                return false;
            }

            account.UpdateAccountFromDTO(updateDTO);
            await _repo.UpdateAsync(account);
            return true;
        }

        public async Task<bool> DeleteAccountAsync(int id, int userId)
        {
            var account = await _repo.GetByIdAsync(id, userId);
            if (account == null)
            {
                return false;
            }

            await _repo.DeleteAsync(account);
            return true;
        }

        public async Task<IEnumerable<TransactionDTO>?> GetTransactionsAsync(int userId, string accountNumber, DateTime startDate, DateTime endDate)
        {
            var accounts = await _repo.GetUserAccountsAsync(userId);
            var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

            if (account == null) return null;

            var transactions = await _repo.GetAccountTransactionsAsync(account.Id, startDate, endDate);

            return transactions.Select(t => new TransactionDTO
            {
                TransactionId = t.TransactionId,
                TransactionName = t.TransactionName,
                Description = t.Description,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Balance = t.Balance,
                TransactionDate = t.TransactionDate
            }).ToList();
        }

        public async Task<DashboardSummaryDto?> GetDashboardSummaryAsync(int userId, string accountNumber, DateTime startDate, DateTime endDate)
        {
            // determine the accounts for the method - either all or the specified one
            var accounts = await _repo.GetUserAccountsAsync(userId);
            var targetAccounts = new List<Models.AccountList>();

            if (accountNumber == "all")
            {
                targetAccounts = accounts.ToList();
            }
            else
            {
                var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
                if (account != null) targetAccounts.Add(account);
            }
            if (!targetAccounts.Any()) return null;


            // get data from db
            var summary = new DashboardSummaryDto();
            decimal currentTotalBalance = targetAccounts.Sum(a => a.Balance);
            var accountIds = targetAccounts.Select(a => a.Id).ToList();
            var allTransactions = await _repo.GetBatchTransactionsAsync(accountIds, startDate, DateTime.Now);


            // add today to list and do the math going backwards in time
            var rawBalanceHistory = new List<(DateTime Date, decimal Balance)>();
            decimal runningBalance = currentTotalBalance;
            rawBalanceHistory.Add((DateTime.Now, runningBalance));

            foreach (var tx in allTransactions)
            {
                // Only add to Income/Expense if it actually happened before the End Date
                if (tx.TransactionDate.Date <= endDate.Date)
                {
                    if (tx.Amount > 0) summary.TotalIncome += tx.Amount;
                    else if (tx.Amount < 0) summary.TotalExpense += Math.Abs(tx.Amount);
                }

                rawBalanceHistory.Add((tx.TransactionDate, runningBalance));
                runningBalance -= tx.Amount;
            }
            rawBalanceHistory.Reverse();


            // determine step size for chart
            var totalDays = (endDate.Date - startDate.Date).TotalDays;
            int stepDays = 1; // Default to daily

            if (totalDays > 90) stepDays = 30;     // Monthly view for large ranges
            else if (totalDays > 30) stepDays = 5; // 5-Day view for medium ranges

            summary.ChartData = new List<ChartDataPointDto>();


            // fill the chart
            decimal lastKnownBalance = runningBalance;
            DateTime lastPlottedDate = startDate.Date;

            for (var dt = startDate.Date; dt <= endDate.Date; dt = dt.AddDays(stepDays))
            {
                lastPlottedDate = dt; // Update memory every loop

                var historicalPoint = rawBalanceHistory.LastOrDefault(b => b.Date.Date <= dt);
                if (historicalPoint.Date != default(DateTime))
                {
                    lastKnownBalance = historicalPoint.Balance;
                }

                string labelFormat = stepDays >= 30 ? "MMM yyyy" : "M/d";

                summary.ChartData.Add(new ChartDataPointDto
                {
                    DateLabel = dt.ToString(labelFormat),
                    Balance = lastKnownBalance
                });
            }



            var finalLabel = endDate.ToString(stepDays >= 30 ? "MMM yyyy" : "M/d");
            if (lastPlottedDate < endDate.Date)
            {
                var finalHistoricalPoint = rawBalanceHistory.LastOrDefault(b => b.Date.Date <= endDate.Date);

                decimal finalBalance = finalHistoricalPoint.Date != default(DateTime)
                    ? finalHistoricalPoint.Balance
                    : currentTotalBalance;

                summary.ChartData.Add(new ChartDataPointDto
                {
                    DateLabel = endDate.ToString(finalLabel),
                    Balance = finalBalance
                });
            }

            summary.NetTotal = summary.TotalIncome - summary.TotalExpense;
            return summary;
        }

        public async Task<DailyLimitDto> GetDailyTransferLimitAsync(int userId, string accountNumber)
        {
            // The Security Check - ensure the account belongs to the user
            var dbAccounts = await _repo.GetUserAccountsAsync(userId);
            var dbAccount = dbAccounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (dbAccount == null)
                throw new Exception("Account not found in your database.");

            // The Business Rules
            decimal dailyTransferLimit = 5000.00m;
            var startOfToday = DateTime.UtcNow.Date;

            // Delegate the math to the transaction repo using the verified account id
            decimal outgoingTransfersToday = await _repo.GetTotalOutgoingTodayAsync(dbAccount.Id, startOfToday);
            decimal totalSpentToday = Math.Abs(outgoingTransfersToday);

            // Return the DTO
            return new DailyLimitDto
            {
                Limit = dailyTransferLimit,
                Used = totalSpentToday,
                Remaining = dailyTransferLimit - totalSpentToday
            };
        }
    }
}
