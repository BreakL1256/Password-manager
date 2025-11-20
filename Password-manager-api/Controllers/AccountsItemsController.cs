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
        private readonly EncryptionAndHashingMethods _tool;
        public AccountsItemsController(AppDbContext context, JWTService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
            _tool = new EncryptionAndHashingMethods();
        }

        // GET: api/AccountsItems
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<AccountsItem>>> GetAccounts()
        //{
        //    return await _context.Accounts.ToListAsync();
        //}

        //// GET: api/AccountsItems/id
        //[HttpGet("{id}")]
        //public async Task<ActionResult<AccountsItem>> GetAccountsItem(long id)
        //{
        //    var accountsItem = await _context.Accounts.FindAsync(id);

        //    if (accountsItem == null)
        //    {
        //        return NotFound();
        //    }

        //    return accountsItem;
        //}

        // POST: api/AccountsItems/login
        //Logs in user and issues token
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AccountsItem>> LoginToCloudAccount(LoginDTO loginCredentials)
        {
            var account = await _context.Accounts.Where(item => item.Email == loginCredentials.Email).FirstOrDefaultAsync();
            bool isFirstbackup = false;

            if (account == null)
            {
                NotFound();
            }
            else if (await Task.Run(() => !_tool.VerifyPassword(loginCredentials.Password, account.Password)))
            {
                BadRequest(new { error = "Invalid Password" });
            }

            var vault = await _context.VaultBackups.Where(item => item.UserId == account.Id && item.VaultOwnerId == loginCredentials.UserIdentifier).FirstOrDefaultAsync();

            if (vault == null)
            {
                isFirstbackup = true;
            }

            string token = _jwtService.GenerateToken(account.Id, account.Email);

            return Ok(new
            {
                UserId = account.Id,
                Email = account.Email,
                Token = token,
                isFirstbackup = isFirstbackup,
            });
        }

        // registers new user and issues token
        // POST: api/AccountsItems/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AccountsItem>> RegisterToCloudAccount(LoginDTO registerCredentials)
        {
            if(await _context.Accounts.AnyAsync(item => item.Email == registerCredentials.Email))
            {
                return BadRequest(new { error = "Email already exists" });
            }

            var passwordHash = await Task.Run(() => _tool.HashString(registerCredentials.Password));

            var newAccount = new AccountsItem
            {
                Email = registerCredentials.Email,
                Password = passwordHash,
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            //var token = _jwtService.GenerateToken(newAccount.Id, newAccount.Email);

            return Ok(new
            {
                Id = newAccount.Id,
                Email = newAccount.Email,
            });
        }

        // DELETE: api/AccountsItems/delete
        //[HttpDelete("delete")]
        //public async Task<IActionResult> DeleteAccountsItem(long id)
        //{
        //    var accountsItem = await _context.Accounts.FindAsync(id);
        //    if (accountsItem == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Accounts.Remove(accountsItem);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //[HttpGet("check/{id}")]
        //public async Task<ActionResult<bool>> AccountsItemExists(long id)
        //{
        //    return await _context.Accounts.AnyAsync(e => e.Id == id);
        //}
    }
}
