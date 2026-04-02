using WebApplication1.Interface;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;
using WebApplication1.Models.External.Vakifbank;

namespace WebApplication1.Services
{
    public class VakifbankSyncService : IVakifbankSyncService
    {
        private readonly IBankIntegrationService _vakifbankService;
        private readonly IAccountRepository _repo;
        private readonly IAuthRepository _authRepo;

        public VakifbankSyncService(IBankIntegrationService vakifbankService, IAccountRepository repo, IAuthRepository authRepo)
        {
            _vakifbankService = vakifbankService;
            _repo = repo;
            _authRepo = authRepo;
        }

        public async Task<IEnumerable<AccountListDTO>> SyncAccountsAsync(int userId)
        {
            var consentId = await _authRepo.GetVakifbankConsentIdAsync(userId);
            if (string.IsNullOrEmpty(consentId))
                throw new Exception("You have not connected your Vakifbank account yet!");

            var externalAccounts = await _vakifbankService.GetAccountsFromBankAsync(userId, consentId);
            var existingAccounts = await _repo.GetUserAccountsAsync(userId);

            foreach (var extAcc in externalAccounts)
            {
                var existingDbAccount = existingAccounts.FirstOrDefault(a => a.AccountNumber == extAcc.AccountNumber);

                if (existingDbAccount == null)
                {
                    var newAccount = new AccountList
                    {
                        UserId = userId,
                        AccountNumber = extAcc.AccountNumber,
                        Balance = extAcc.Balance,
                        RemainingBalance = extAcc.RemainingBalance,
                        IBAN = extAcc.IBAN,
                        CurrencyCode = extAcc.CurrencyCode,
                        AccountStatus = extAcc.AccountStatus,
                        AccountType = extAcc.AccountType,
                        LastTransactionDate = extAcc.LastTransactionDate,
                        ProviderName = "Vakifbank"
                    };
                    await _repo.CreateAsync(newAccount);
                }
                else
                {
                    existingDbAccount.Balance = extAcc.Balance;
                    existingDbAccount.RemainingBalance = extAcc.RemainingBalance;
                    existingDbAccount.LastTransactionDate = extAcc.LastTransactionDate;
                    await _repo.UpdateAsync(existingDbAccount);
                }
            }

            return externalAccounts;
        }

        public async Task<AccountDetailDTO> GetAndSyncAccountDetailAsync(int userId, string accountNumber)
        {
            var userAccounts = await _repo.GetUserAccountsAsync(userId);
            var dbAccount = userAccounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

            if (dbAccount == null) throw new Exception("Account not found in your database.");

            if (dbAccount.ProviderName == "Internal")
            {
                return new AccountDetailDTO
                {
                    AccountNumber = dbAccount.AccountNumber,
                    Balance = dbAccount.Balance,
                    RemainingBalance = dbAccount.RemainingBalance,
                    IBAN = dbAccount.IBAN,
                    CurrencyCode = dbAccount.CurrencyCode,
                    AccountStatus = dbAccount.AccountStatus,
                    AccountType = dbAccount.AccountType,
                    OpeningDate = dbAccount.OpeningDate,
                    CustomerNumber = dbAccount.CustomerNumber!,
                    BranchCode = dbAccount.BranchCode!,
                    ProviderName = dbAccount.ProviderName
                };
            }

            var consentId = await _authRepo.GetVakifbankConsentIdAsync(userId);
            var detail = await _vakifbankService.GetAccountDetailAsync(accountNumber, consentId);
            if (detail == null) throw new Exception("Account details not found at the bank.");

            dbAccount.OpeningDate = detail.OpeningDate;
            dbAccount.CustomerNumber = detail.CustomerNumber;
            dbAccount.BranchCode = detail.BranchCode;
            dbAccount.Balance = detail.Balance;
            dbAccount.RemainingBalance = detail.RemainingBalance;

            await _repo.UpdateAsync(dbAccount);

            return detail;
        }

        public async Task<IEnumerable<TransactionDTO>> SyncTransactionsAsync(int userId, string accountNumber, DateTime startDate, DateTime endDate)
        {
            var userAccounts = await _repo.GetUserAccountsAsync(userId);
            var dbAccount = userAccounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

            if (dbAccount == null) throw new Exception("Account not found in your database. Please sync accounts first.");
            if (dbAccount.ProviderName == "Internal") throw new Exception("Internal accounts cannot be synced with external banks.");

            var consentId = await _authRepo.GetVakifbankConsentIdAsync(userId);
            var externalTransactions = await _vakifbankService.GetAccountTransactionsAsync(accountNumber, startDate, endDate, consentId);
            var existingTxIds = await _repo.GetExistingTransactionIdsAsync(dbAccount.Id, startDate, endDate);

            var newTransactions = new List<AccountTransaction>();
            foreach (var extTx in externalTransactions)
            {
                if (!existingTxIds.Contains(extTx.TransactionId))
                {
                    newTransactions.Add(new AccountTransaction
                    {
                        AccountListId = dbAccount.Id,
                        TransactionId = extTx.TransactionId,
                        TransactionName = extTx.TransactionName,
                        Description = extTx.Description,
                        TransactionType = extTx.TransactionType,
                        Amount = extTx.Amount,
                        Balance = extTx.Balance,
                        TransactionDate = extTx.TransactionDate
                    });
                }
            }

            if (newTransactions.Count > 0)
            {
                await _repo.SaveTransactionsAsync(newTransactions);
            }

            return externalTransactions;
        }

        public async Task<byte[]> GetReceiptPdfAsync(int userId, string accountNumber, string transactionId)
        {
            var consentId = await _authRepo.GetVakifbankConsentIdAsync(userId);
            if (string.IsNullOrEmpty(consentId)) throw new Exception("You have not connected your Vakifbank account yet!");

            return await _vakifbankService.GetReceiptPdfAsync(transactionId, accountNumber, consentId);
        }

        public async Task<decimal> CalculateCurrencyAsync(string sourceCurrency, decimal amount, string targetCurrency)
        {
            return await _vakifbankService.CalculateCurrencyAsync(sourceCurrency, amount, targetCurrency);
        }
        public async Task<DepositProductResponse> GetDepositProductsAsync()
        {
            return await _vakifbankService.GetDepositProductsAsync();
        }
        public async Task<BranchListResponse> GetBranchListAsync(string? cityCode = null, string? districtCode = null)
        {
            return await _vakifbankService.GetBranchListAsync(cityCode, districtCode);
        }

        public async Task<DepositCalculatorResponse> CalculateDepositAsync(DepositCalculatorRequest request)
        {
            return await _vakifbankService.CalculateDepositAsync(request);
        }
        public async Task<ATMListResponse> GetATMListAsync(string? cityCode = null, string? districtCode = null)
        {
            return await _vakifbankService.GetATMListAsync(cityCode, districtCode);
        }
        public async Task<CityListResponse> GetCityListAsync()
        {
            return await _vakifbankService.GetCityListAsync();
        }
        public async Task<DistrictListResponse> GetDistrictListAsync(string cityCode)
        {
            return await _vakifbankService.GetDistrictListAsync(cityCode);
        }
        public async Task<NeighborhoodListResponse> GetNeighborhoodListAsync(string districtCode)
        {
            return await _vakifbankService.GetNeighborhoodListAsync(districtCode);
        }
    }
}
