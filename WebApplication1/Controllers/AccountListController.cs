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
        private readonly IAccountRepository _repo; // Changed from DBContext

        public AccountListController(IAccountRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("get-accounts-list")] //api/AccountList
        public async Task<ActionResult<IEnumerable<AccountListDTO>>> GetAll()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            var userId = int.Parse(userIdClaim.Value);
            var accounts = await _repo.GetUserAccountsAsync(userId);

            var accountDtos = accounts.Select(s => s.ToAccountDto()).ToList();

            return Ok(accountDtos);


        }

        [HttpGet("{id}get-account")] //api/AccountList/{id}
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

            var responseDto = new AccountListDTO
            {
                AccountNumber = newAccount.AccountNumber,
                Balance = newAccount.Balance,
                RemainingBalance = newAccount.RemainingBalance,
                IBAN = newAccount.IBAN,
                CurrencyCode = newAccount.CurrencyCode,
                AccountStatus = newAccount.AccountStatus,
                LastTransactionDate = newAccount.LastTransactionDate,
                AccountType = newAccount.AccountType
            };

            return CreatedAtAction(nameof(GetById), new { id = newAccount.Id }, responseDto);

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
    }
}
