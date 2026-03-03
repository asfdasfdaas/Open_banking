using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public VakifbankController(IBankIntegrationService vakifbankService)
        {
            _vakifbankService = vakifbankService;
        }

        [HttpPost("vakif-accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid token.");
                }

                var accounts = await _vakifbankService.GetAccountsFromBankAsync(userId);
                return Ok(accounts);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching accounts.");
            }
        }
    }
}