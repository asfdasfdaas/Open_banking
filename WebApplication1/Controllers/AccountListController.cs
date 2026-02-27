using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Interfaces;
using WebApplication1.Mapper;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountListController : ControllerBase
    {
        private readonly IAccountRepository _repo; // Changed from DBContext

        public AccountListController(IAccountRepository repo)
        {
            _repo = repo;
        }

        [HttpGet] //api/AccountList
        public async Task<ActionResult<IEnumerable<AccountListDTO>>> GetAll()
        {
            var accounts = await _repo.GetAllAsync();

            var accountDtos = accounts.Select(s => s.ToAccountDto()).ToList();

            return Ok(accountDtos);


        }

        [HttpGet("{id}")] //api/AccountList/{id}
        public async Task<ActionResult<AccountListDTO>> GetById(int id)
        {
            var account = await _repo.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            var accountDto = account.ToAccountDto();

            return Ok(accountDto);
        }

        [HttpPost]
        public async Task<ActionResult<AccountListDTO>> CreateAccount([FromBody] AccountCreateDTO createDTO)
        {
            var newAccount = createDTO.ToAccountFromCreateDTO();

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount([FromRoute] int id, [FromBody] AccountUpdateDTO updateDTO)
        {
            var Account = await _repo.GetByIdAsync(id);

            if (Account == null)
            {
                return NotFound();
            }

            Account.UpdateAccountFromDTO(updateDTO);

            await _repo.UpdateAsync(Account);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<AccountListDTO>> DeleteAccount([FromRoute] int id) 
        { 
            var Account = await _repo.GetByIdAsync(id);

            if (Account == null)
            {
                return NotFound();
            }
            await _repo.DeleteAsync(Account);
            return NoContent();
        }
    }
}
