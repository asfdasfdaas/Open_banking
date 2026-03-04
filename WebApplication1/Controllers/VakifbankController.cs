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
        public VakifbankController(IBankIntegrationService vakifbankService, IAccountRepository repo)
        {
            _vakifbankService = vakifbankService;
            _repo = repo;
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
                var detail = await _vakifbankService.GetAccountDetailAsync(accountNumber);

                if (detail == null) return NotFound("Account details not found at the bank.");

                return Ok(detail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred fetching account details.", details = ex.Message });
            }
        }
    }
}