using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Interface;
using WebApplication1.Models.DTOs;
using WebApplication1.Filters;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountListController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountListController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("get-accounts-list")]
        public async Task<ActionResult<IEnumerable<AccountListDTO>>> GetAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            
            var accountDtos = await _accountService.GetAllAsync(userId);

            return Ok(accountDtos);
        }

        [HttpGet("{id}get-account")] 
        public async Task<ActionResult<AccountListDTO>> GetById([FromRoute]int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            
            var accountDto = await _accountService.GetByIdAsync(id, userId);
            if (accountDto == null)
            {
                return NotFound();
            }

            return Ok(accountDto);
        }

        [HttpPost("create-account")]
        public async Task<ActionResult<AccountListDTO>> CreateAccount([FromBody] AccountCreateDTO createDTO)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            
            var result = await _accountService.CreateAccountAsync(createDTO, userId);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result.Dto);
        }

        [HttpPost("transfer")]
        [Idempotent]
        public async Task<IActionResult> TransferInternal([FromBody] TransferDTO transferDto)
        {
            // Identify the user making the request from their secure JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            try
            {
                // Hand the data off to service
                var success = await _accountService.TransferInternalAsync(userId, transferDto);

                if (success)
                {
                    return Ok(new { message = "Transfer completed successfully." });
                }

                return BadRequest(new { message = "Transfer failed due to an unknown error." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}update-account")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int id, [FromBody] AccountUpdateDTO updateDTO)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            
            var updated = await _accountService.UpdateAccountAsync(id, userId, updateDTO);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}delete-account")]
        public async Task<ActionResult<AccountListDTO>> DeleteAccount([FromRoute] int id) 
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            
            var deleted = await _accountService.DeleteAccountAsync(id, userId);

            if (!deleted)
            {
                return NotFound();
            }
            
            return NoContent();
        }

        [HttpGet("{accountNumber}/transactions")]
        public async Task<IActionResult> GetTransactions(string accountNumber, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var transactionDtos = await _accountService.GetTransactionsAsync(userId, accountNumber, startDate, endDate);

            if (transactionDtos == null) return NotFound("Account not found.");

            // Return the clean DTOs
            return Ok(transactionDtos);
        }

        [HttpGet("dashboard/summary/{accountNumber}")]
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary(string accountNumber, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);

            var summary = await _accountService.GetDashboardSummaryAsync(userId, accountNumber, startDate, endDate);

            if (summary == null) return NotFound("Account not found.");

            return Ok(summary);
        }
    }
}