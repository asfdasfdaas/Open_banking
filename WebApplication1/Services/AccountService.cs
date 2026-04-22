using WebApplication1.Interface;
using WebApplication1.Mapper;
using WebApplication1.Models.DTOs;
using System.Security.Principal;

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
            var newAccount = createDTO.ToAccountFromCreateDTO();
            newAccount.UserId = userId;

            await _repo.CreateAsync(newAccount);

            return (newAccount.ToAccountDto(), newAccount.Id);
        }

        public async Task<bool> TransferInternalAsync(int userId, TransferDTO transferDto)
        {
            return await _repo.TransferMoneyInternalAsync(userId, transferDto);
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

            var summary = new DashboardSummaryDto();

            decimal currentTotalBalance = targetAccounts.Sum(a => a.Balance);
            var accountIds = targetAccounts.Select(a => a.Id).ToList();

            var allTransactions = await _repo.GetBatchTransactionsAsync(accountIds, startDate, DateTime.Now);

            var rawBalanceHistory = new List<(DateTime Date, decimal Balance)>();
            decimal runningBalance = currentTotalBalance;

            rawBalanceHistory.Add((DateTime.Now, runningBalance));

            foreach (var tx in allTransactions)
            {
                // Only add to Income/Expense if it actually happened before the End Date!
                if (tx.TransactionDate.Date <= endDate.Date)
                {
                    if (tx.Amount > 0) summary.TotalIncome += tx.Amount;
                    else if (tx.Amount < 0) summary.TotalExpense += Math.Abs(tx.Amount);
                }

                
                rawBalanceHistory.Add((tx.TransactionDate, runningBalance));
                runningBalance -= tx.Amount;
            }

            rawBalanceHistory.Reverse();

            var totalDays = (endDate.Date - startDate.Date).TotalDays;
            int stepDays = 1; // Default to daily

            if (totalDays > 90) stepDays = 30;     // Monthly view for large ranges
            else if (totalDays > 30) stepDays = 5; // 5-Day view for medium ranges

            summary.ChartData = new List<ChartDataPointDto>();

            decimal lastKnownBalance = runningBalance;


            for (var dt = startDate.Date; dt <= endDate.Date; dt = dt.AddDays(stepDays))
            {
                // Search our history for the most recent transaction that happened on or before this timeline date
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
            if (summary.ChartData.LastOrDefault()?.DateLabel != finalLabel)
            {
                summary.ChartData.Add(new ChartDataPointDto { DateLabel = finalLabel, Balance = currentTotalBalance });
            }

            summary.NetTotal = summary.TotalIncome - summary.TotalExpense;
            return summary;
        }
    }
}
