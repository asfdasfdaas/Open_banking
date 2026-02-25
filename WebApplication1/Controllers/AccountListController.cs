using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.DTOs;
namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountListController : ControllerBase
    {
        private readonly ApplicationDBContext _db;

        public AccountListController(ApplicationDBContext db)
        {
            _db = db;
        }
        [HttpGet] //api/AccountList
        public async Task<ActionResult<IEnumerable<AccountListDTO>>> GetAll()
        {
            //unused code just in case
            //
            //return await _db.AccountLists.ToListAsync();

            //var accountLists = _db.AccountLists.ToList();
            //return Ok(accountLists);

            var accounts = await _db.AccountLists.ToListAsync();

            var accountDtos = accounts.Select(a => new AccountListDTO
            {
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                RemainingBalance = a.RemainingBalance,
                IBAN = a.IBAN,
                CurrencyCode = a.CurrencyCode,
                AccountStatus = a.AccountStatus,
                LastTransactionDate = a.LastTransactionDate,
                AccountType = a.AccountType
            }).ToList();

            return Ok(accountDtos);


        }
        [HttpGet("{id}")] //api/AccountList/{id}
        public async Task<ActionResult<AccountListDTO>> GetById(int id)
        {
            var account = await _db.AccountLists.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            var accountDto = new AccountListDTO
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                RemainingBalance = account.RemainingBalance,
                IBAN = account.IBAN,
                CurrencyCode = account.CurrencyCode,
                AccountStatus = account.AccountStatus,
                LastTransactionDate = account.LastTransactionDate,
                AccountType = account.AccountType
            };

            return Ok(accountDto);
        }
        [HttpPost]
        public async Task<ActionResult<AccountListDTO>> CreateAccount([FromBody] AccountCreateDTO createDTO)
        {
            var newAccount = new AccountList
            {
                AccountNumber = createDTO.AccountNumber,
                IBAN = createDTO.IBAN,
                CurrencyCode = createDTO.CurrencyCode,
                AccountType = createDTO.AccountType,

                Balance = 0,
                RemainingBalance = 0,
                AccountStatus = "A",
                LastTransactionDate = DateTime.Now
            };
            _db.AccountLists.Add(newAccount);
            await _db.SaveChangesAsync();

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
        public async Task<ActionResult<AccountListDTO>> UpdateAccount([FromRoute] int id, [FromBody] AccountUpdateDTO updateDTO)
        {
            var Account = await _db.AccountLists.FindAsync(id);

            if (Account == null)
            {
                return NotFound();
            }

            Account.CurrencyCode = updateDTO.CurrencyCode;
            Account.AccountStatus = updateDTO.AccountStatus;
            Account.AccountType = updateDTO.AccountType;

            await _db.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<AccountListDTO>> DeleteAccount([FromRoute] int id) 
        { 
            var Account = await _db.AccountLists.FindAsync(id);

            if (Account == null)
            {
                return NotFound();
            }
            _db.AccountLists.Remove(Account);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
