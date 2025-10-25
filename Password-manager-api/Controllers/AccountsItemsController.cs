using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Password_manager_api.Entities;
using Password_manager_api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Password_manager_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountsItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JWTService _jwtService;

        public AccountsItemsController(AppDbContext context, JWTService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // GET: api/AccountsItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountsItem>>> GetAccounts()
        {
            return await _context.Accounts.ToListAsync();
        }

        // GET: api/AccountsItems/id
        [HttpGet("{id}")]
        public async Task<ActionResult<AccountsItem>> GetAccountsItem(long id)
        {
            var accountsItem = await _context.Accounts.FindAsync(id);

            if (accountsItem == null)
            {
                return NotFound();
            }

            return accountsItem;
        }

        // POST: api/AccountsItems/login
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AccountsItem>> LoginToCloudAccount(LoginDTO loginCredentials)
        {
            var account = await _context.Accounts.Where(item => item.Email == loginCredentials.Email).FirstOrDefaultAsync();

            if (account == null)
            {
                NotFound();
            }
            else if (account.Password != loginCredentials.Password)
            {
                BadRequest(new { error = "Invalid Password" });
            }

            var token = _jwtService.GenerateToken(account.Id, account.Email);

            return Ok(new
            {
                UserId = account.Id,
                Email = account.Email,
                Token = token
            });
        }

        // POST: api/AccountsItems/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AccountsItem>> RegisterToCloudAccount(LoginDTO registerCredentials)
        {
            if(await _context.Accounts.AnyAsync(item => item.Email == registerCredentials.Email))
            {
                return BadRequest(new { error = "Email already exists" });
            }

            var newAccount = new AccountsItem
            {
                Email = registerCredentials.Email,
                Password = registerCredentials.Password,
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(newAccount.Id, newAccount.Email);

            return Ok(new
            {
                Id = newAccount.Id,
                Email = newAccount.Email,
                Token = token
            });
        }

        // DELETE: api/AccountsItems/delete
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccountsItem(long id)
        {
            var accountsItem = await _context.Accounts.FindAsync(id);
            if (accountsItem == null)
            {
                return NotFound();
            }

            _context.Accounts.Remove(accountsItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("check/{id}")]
        public async Task<ActionResult<bool>> AccountsItemExists(long id)
        {
            return await _context.Accounts.AnyAsync(e => e.Id == id);
        }
    }
}
