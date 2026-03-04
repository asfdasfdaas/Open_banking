using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interfaces;
using WebApplication1.Models;
using WebApplication1.Models.External.Vakifbank;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VakifbankController : ControllerBase
    {
        private readonly IBankIntegrationService _vakifbankService;
        private readonly IAccountRepository _repo;
        private readonly Data.ApplicationDBContext _db;
        public VakifbankController(IBankIntegrationService vakifbankService, IAccountRepository repo, Data.ApplicationDBContext db)
        {
            _vakifbankService = vakifbankService;
            _repo = repo;
            _db = db;
        }

        [HttpPost("vakif-accounts")]
        public async Task<IActionResult> SyncAccounts()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized("Invalid token.");

                // 1. Fetch live data from Vakifbank
                var externalAccounts = await _vakifbankService.GetAccountsFromBankAsync(userId);

                // 2. Fetch the user's current accounts from your database
                var existingAccounts = await _repo.GetUserAccountsAsync(userId);

                // 3. The Sync Loop
                foreach (var extAcc in externalAccounts)
                {
                    // Check if we already saved this specific account number
                    var existingDbAccount = existingAccounts.FirstOrDefault(a => a.AccountNumber == extAcc.AccountNumber);

                    if (existingDbAccount == null)
                    {
                        // INSERT: It's a brand new account!
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
                            ProviderName = "Vakifbank" // Tag it so we know where it came from
                        };
                        await _repo.CreateAsync(newAccount);
                    }
                    else
                    {
                        // UPDATE: We already have it, just update the live balance!
                        existingDbAccount.Balance = extAcc.Balance;
                        existingDbAccount.RemainingBalance = extAcc.RemainingBalance;
                        existingDbAccount.LastTransactionDate = extAcc.LastTransactionDate;

                        await _repo.UpdateAsync(existingDbAccount);
                    }
                }

                // Return the fresh data to the screen
                return Ok(externalAccounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during sync.", details = ex.Message });
            }
        }

        [HttpGet("account-detail/{accountNumber}")]
        public async Task<IActionResult> GetAccountDetail([FromRoute] string accountNumber)
        {
            try
            {
                // 1. Get the current user
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized("Invalid token.");

                // 2. Fetch the live detail data from the bank
                var detail = await _vakifbankService.GetAccountDetailAsync(accountNumber);
                if (detail == null) return NotFound("Account details not found at the bank.");

                // 3. Find this specific account in your database
                var userAccounts = await _repo.GetUserAccountsAsync(userId);
                var dbAccount = userAccounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

                if (dbAccount != null)
                {
                    // 4. Update the database record with the new 3 fields!
                    dbAccount.OpeningDate = detail.OpeningDate;
                    dbAccount.CustomerNumber = detail.CustomerNumber;
                    dbAccount.BranchCode = detail.BranchCode;

                    // Also update the balances since we just got fresh data
                    dbAccount.Balance = detail.Balance;
                    dbAccount.RemainingBalance = detail.RemainingBalance;

                    await _repo.UpdateAsync(dbAccount);
                }

                // Return the full detail to Swagger/Frontend
                return Ok(detail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred fetching account details.", details = ex.Message });
            }
        }

        [HttpPost("account-transactions/{accountNumber}")]
        public async Task<IActionResult> SyncTransactions([FromRoute] string accountNumber, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized("Invalid token.");

                // 1. Find the internal Database Account so we have its ID
                var userAccounts = await _repo.GetUserAccountsAsync(userId);
                var dbAccount = userAccounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

                if (dbAccount == null) return NotFound("Account not found in your database. Please sync accounts first.");

                // 2. Fetch live transactions from the bank
                var externalTransactions = await _vakifbankService.GetAccountTransactionsAsync(accountNumber, startDate, endDate);

                // 3. Get transactions we ALREADY saved for this account
                var existingTxIds = _db.Set<AccountTransaction>()
                                       .Where(t => t.AccountListId == dbAccount.Id)
                                       .Select(t => t.TransactionId)
                                       .ToList();

                // 4. Save only the NEW transactions
                foreach (var extTx in externalTransactions)
                {
                    if (!existingTxIds.Contains(extTx.TransactionId))
                    {
                        var newTx = new AccountTransaction
                        {
                            AccountListId = dbAccount.Id,
                            TransactionId = extTx.TransactionId,
                            TransactionName = extTx.TransactionName,
                            Description = extTx.Description,
                            TransactionType = extTx.TransactionType,
                            Amount = extTx.Amount,
                            Balance = extTx.Balance,
                            TransactionDate = extTx.TransactionDate
                        };
                        _db.Add(newTx);
                    }
                }

                await _db.SaveChangesAsync();

                return Ok(externalTransactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred syncing transactions.", details = ex.Message });
            }
        }
    }
}