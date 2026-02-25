using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ActionResult<IEnumerable<AccountList>>> GetAll()
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
        public async Task<ActionResult<AccountList>> GetById(int id)
        {
            var account = await _db.AccountLists.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }
        //[HttpPost]
        //public async Task<>
    }
}
