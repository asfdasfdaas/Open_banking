using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Interface;
using WebApplication1.Models.External.Vakifbank;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/banks")]
    public class BanksController : ControllerBase
    {
        private readonly IVakifbankSyncService _vakifbankSyncService;

        public BanksController(IVakifbankSyncService vakifbankSyncService)
        {
            _vakifbankSyncService = vakifbankSyncService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) throw new UnauthorizedAccessException("Invalid token.");
            return userId;
        }

        private static string NormalizeProvider(string provider) => (provider ?? string.Empty).Trim().ToLowerInvariant();

        [HttpPost("{provider}/accounts/sync")]
        public async Task<IActionResult> SyncAccounts([FromRoute] string provider)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank")
            {
                var accounts = await _vakifbankSyncService.SyncAccountsAsync(GetUserId());
                return Ok(accounts);
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support account sync." });
        }

        [HttpGet("{provider}/accounts/{accountNumber}")]
        public async Task<IActionResult> GetAccountDetail([FromRoute] string provider, [FromRoute] string accountNumber)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank" or "internal")
            {
                // Current implementation resolves provider from the stored account row.
                // For now, this returns Internal details without calling any external bank API.
                var detail = await _vakifbankSyncService.GetAndSyncAccountDetailAsync(GetUserId(), accountNumber);
                return Ok(detail);
            }

            return BadRequest(new { message = $"Provider '{provider}' is not supported." });
        }

        [HttpPost("{provider}/accounts/{accountNumber}/transactions/sync")]
        public async Task<IActionResult> SyncTransactions(
            [FromRoute] string provider,
            [FromRoute] string accountNumber,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank")
            {
                var transactions = await _vakifbankSyncService.SyncTransactionsAsync(GetUserId(), accountNumber, startDate, endDate);
                return Ok(transactions);
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support transaction sync." });
        }

        [HttpGet("{provider}/accounts/{accountNumber}/receipt/{transactionId}")]
        public async Task<IActionResult> DownloadReceipt([FromRoute] string provider, [FromRoute] string accountNumber, [FromRoute] string transactionId)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank")
            {
                byte[] pdfBytes = await _vakifbankSyncService.GetReceiptPdfAsync(GetUserId(), accountNumber, transactionId);
                return File(pdfBytes, "application/pdf", $"Receipt_{transactionId}.pdf");
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support receipts." });
        }

        [AllowAnonymous]
        [HttpPost("{provider}/currency-calculator")]
        public async Task<IActionResult> CalculateCurrency(
            [FromRoute] string provider,
            [FromQuery] string sourceCurrency,
            [FromQuery] decimal amount,
            [FromQuery] string targetCurrency)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank")
            {
                var convertedAmount = await _vakifbankSyncService.CalculateCurrencyAsync(sourceCurrency, amount, targetCurrency);
                return Ok(new { convertedAmount = convertedAmount });
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support currency conversion." });
        }

        [AllowAnonymous]
        [HttpPost("{provider}/deposit-products")]
        public async Task<IActionResult> GetDepositProducts([FromRoute] string provider)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank")
            {
                var products = await _vakifbankSyncService.GetDepositProductsAsync();
                return Ok(products.Data.DepositProduct);
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support deposit products." });
        }

        [AllowAnonymous]
        [HttpPost("{provider}/deposit-calculator")]
        public async Task<IActionResult> CalculateDeposit([FromRoute] string provider, [FromBody] DepositCalculatorRequest request)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank")
            {
                var result = await _vakifbankSyncService.CalculateDepositAsync(request);
                return Ok(result.Data.Deposit);
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support deposit calculations." });
        }

        [AllowAnonymous]
        [HttpPost("{provider}/cities")]
        public async Task<IActionResult> GetCities([FromRoute] string provider)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank")
            {
                var cities = await _vakifbankSyncService.GetCityListAsync();
                return Ok(cities.Data.City);
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support cities." });
        }

        [AllowAnonymous]
        [HttpPost("{provider}/districts")]
        public async Task<IActionResult> GetDistricts([FromRoute] string provider, [FromQuery] string cityCode)
        {
            provider = NormalizeProvider(provider);

            if (string.IsNullOrWhiteSpace(cityCode))
            {
                return BadRequest(new { message = "CityCode is required." });
            }

            if (provider is "vakifbank" or "vakıfbank")
            {
                var districts = await _vakifbankSyncService.GetDistrictListAsync(cityCode);
                return Ok(districts.Data.District);
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support districts." });
        }

        [AllowAnonymous]
        [HttpPost("{provider}/branches")]
        public async Task<IActionResult> GetBranches([FromRoute] string provider, [FromQuery] string? cityCode = null, [FromQuery] string? districtCode = null)
        {
            provider = NormalizeProvider(provider);

            if (provider is "vakifbank" or "vakıfbank")
            {
                var branches = await _vakifbankSyncService.GetBranchListAsync(cityCode, districtCode);
                return Ok(branches.Data.Branch);
            }

            return BadRequest(new { message = $"Provider '{provider}' does not support branches." });
        }
    }
}

