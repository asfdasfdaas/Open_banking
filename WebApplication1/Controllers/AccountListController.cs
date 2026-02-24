using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountList>>> GetAll()
        {
            //unused code just in case
            //
            //return await _db.AccountLists.ToListAsync();

            //var accountLists = _db.AccountLists.ToList();
            //return Ok(accountLists);

            var accounts = await _db.AccountLists.ToListAsync();

            return Ok(accounts);

            
        }
    }
}
