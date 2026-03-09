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
    }
}