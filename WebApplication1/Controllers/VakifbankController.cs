using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interface;
using WebApplication1.Interfaces;
using WebApplication1.Models.External.Vakifbank;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VakifbankController : ControllerBase
    {
        private readonly IVakifbankSyncService _syncService;

        public VakifbankController(IVakifbankSyncService syncService)
        {
            _syncService = syncService;
        }
        private int GetUserId()
        {

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId)) throw new UnauthorizedAccessException("Invalid token.");
                return userId;

        }

        [HttpPost("vakif-accounts")]
        public async Task<IActionResult> SyncAccounts()
        {
            var accounts = await _syncService.SyncAccountsAsync(GetUserId());
            return Ok(accounts);
        }

        [AllowAnonymous]
        [HttpPost("currency-calculator")]
        public async Task<IActionResult> CalculateCurrency(
            [FromQuery] string sourceCurrency,
            [FromQuery] decimal amount,
            [FromQuery] string targetCurrency)
        {
            try
            {
                var convertedAmount = await _syncService.CalculateCurrencyAsync(sourceCurrency, amount, targetCurrency);
                return Ok(new { convertedAmount = convertedAmount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }


        }

        [HttpGet("account-detail/{accountNumber}")]
        public async Task<IActionResult> GetAccountDetail([FromRoute] string accountNumber)
        {
            var detail = await _syncService.GetAndSyncAccountDetailAsync(GetUserId(), accountNumber);
            return Ok(detail);
        }

        [HttpPost("account-transactions/{accountNumber}")]
        public async Task<IActionResult> SyncTransactions([FromRoute] string accountNumber, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var transactions = await _syncService.SyncTransactionsAsync(GetUserId(), accountNumber, startDate, endDate);
            return Ok(transactions);
        }

        [HttpGet("receipt/{accountNumber}/{transactionId}")]
        public async Task<IActionResult> DownloadReceipt([FromRoute] string accountNumber, [FromRoute] string transactionId)
        {
            byte[] pdfBytes = await _syncService.GetReceiptPdfAsync(GetUserId(), accountNumber, transactionId);
            return File(pdfBytes, "application/pdf", $"Receipt_{transactionId}.pdf");
        }

        [AllowAnonymous]
        [HttpPost("deposit-products")]
        public async Task<IActionResult> GetDepositProducts()
        {
            try
            {
                var products = await _syncService.GetDepositProductsAsync();

                // We return just the list of products to make the Angular side cleaner
                return Ok(products.Data.DepositProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("branches")]
        public async Task<IActionResult> GetBranches([FromQuery] string? cityCode = null, [FromQuery] string? districtCode = null)
        {
            try
            {
                var branches = await _syncService.GetBranchListAsync(cityCode, districtCode);

                // Return just the array of branches for cleaner frontend consumption
                return Ok(branches.Data.Branch);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [AllowAnonymous]
        [HttpPost("deposit-calculator")]
        public async Task<IActionResult> CalculateDeposit([FromBody] DepositCalculatorRequest request)
        {
            try
            {
                var result = await _syncService.CalculateDepositAsync(request);

                // Return just the pure math details so Angular can read it easily
                return Ok(result.Data.Deposit);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [AllowAnonymous]
        [HttpPost("atms")]
        public async Task<IActionResult> GetATMs([FromQuery] string? cityCode = null, [FromQuery] string? districtCode = null)
        {
            try
            {
                var atms = await _syncService.GetATMListAsync(cityCode, districtCode);

                // Return just the array of ATMs 
                return Ok(atms.Data.ATM);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}