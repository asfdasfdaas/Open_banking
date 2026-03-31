using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Interfaces;
using WebApplication1.Mapper;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;

namespace WebApplication1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountListController : ControllerBase
    {
        private readonly IAccountRepository _repo;

        public AccountListController(IAccountRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("get-accounts-list")] 
        public async Task<ActionResult<IEnumerable<AccountListDTO>>> GetAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            var accounts = await _repo.GetUserAccountsAsync(userId);

            var accountDtos = accounts.Select(s => s.ToAccountDto()).ToList();

            return Ok(accountDtos);


        }

        [HttpGet("{id}get-account")] 
        public async Task<ActionResult<AccountListDTO>> GetById(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var account = await _repo.GetByIdAsync(id,userId);
            if (account == null)
            {
                return NotFound();
            }
            var accountDto = account.ToAccountDto();

            return Ok(accountDto);
        }

        [HttpPost("create-account")]
        public async Task<ActionResult<AccountListDTO>> CreateAccount([FromBody] AccountCreateDTO createDTO)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var newAccount = createDTO.ToAccountFromCreateDTO();

            newAccount.UserId = userId;

            await _repo.CreateAsync(newAccount);

            var responseDto = newAccount.ToAccountDto();

            return CreatedAtAction(nameof(GetById), new { id = newAccount.Id }, responseDto);

        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferInternal([FromBody] TransferDTO transferDto)
        {
            // 1. Identify the user making the request from their secure JWT token
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            try
            {
                // 2. Hand the data off to repository engine
                var success = await _repo.TransferMoneyInternalAsync(userId, transferDto);

                if (success)
                {
                    // 3. Return a clean 200 OK with a success message
                    return Ok(new { message = "Transfer completed successfully." });
                }

                return BadRequest(new { message = "Transfer failed due to an unknown error." });
            }
            catch (Exception ex)
            {
                // 4. Catch the specific business logic errors 
                // that manually threw in the Repository, and send them to the frontend
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}update-account")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int id, [FromBody] AccountUpdateDTO updateDTO)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var Account = await _repo.GetByIdAsync(id,userId);

            if (Account == null)
            {
                return NotFound();
            }

            Account.UpdateAccountFromDTO(updateDTO);

            await _repo.UpdateAsync(Account);

            return NoContent();
        }

        [HttpDelete("{id}delete-account")]
        public async Task<ActionResult<AccountListDTO>> DeleteAccount([FromRoute] int id) 
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var Account = await _repo.GetByIdAsync(id,userId);

            if (Account == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(Account);
            return NoContent();
        }

        [HttpGet("{accountNumber}/transactions")]
        public async Task<IActionResult> GetTransactions(string accountNumber, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);

            // 1. Find the specific account for this user
            var accounts = await _repo.GetUserAccountsAsync(userId);
            var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);

            if (account == null) return NotFound("Account not found.");

            // 2. Fetch the transactions from the local database
            var transactions = await _repo.GetAccountTransactionsAsync(account.Id, startDate, endDate);

            var transactionDtos = transactions.Select(t => new TransactionDTO
            {
                TransactionId = t.TransactionId,
                TransactionName = t.TransactionName,
                Description = t.Description,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Balance = t.Balance,
                TransactionDate = t.TransactionDate
            });

            // Return the clean DTOs
            return Ok(transactionDtos);
        }
    }
}