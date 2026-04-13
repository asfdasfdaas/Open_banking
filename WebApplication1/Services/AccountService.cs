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
            var allTransactions = await _repo.GetBatchTransactionsAsync(accountIds, startDate, endDate);

            var chartPoints = new List<ChartDataPointDto>();
            decimal runningBalance = currentTotalBalance;

            // Add the current total balance as the most recent point
            chartPoints.Add(new ChartDataPointDto { DateLabel = DateTime.Now.ToString("M/d"), Balance = runningBalance });

            foreach (var tx in allTransactions)
            {
                if (tx.Amount > 0)
                    summary.TotalIncome += tx.Amount;
                else if (tx.Amount < 0)
                    summary.TotalExpense += Math.Abs(tx.Amount);

                runningBalance -= tx.Amount;

                chartPoints.Add(new ChartDataPointDto
                {
                    DateLabel = tx.TransactionDate.ToString("M/d"),
                    Balance = runningBalance
                });
            }

            // Reverse to make it chronological (oldest to newest)
            chartPoints.Reverse();

            // Group by Day 
            summary.ChartData = chartPoints
                .GroupBy(cp => cp.DateLabel)
                .Select(group => new ChartDataPointDto
                {
                    DateLabel = group.Key,
                    Balance = group.Last().Balance // Grab the final balance of that specific day
                })
                .ToList();

            summary.NetTotal = summary.TotalIncome - summary.TotalExpense;
            return summary;
        }
    }
}
