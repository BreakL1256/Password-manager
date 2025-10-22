using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Password_manager_api.Models;

namespace Password_manager_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AccountsItemsController(AppDbContext context)
        {
            _context = context;
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

        // POST: api/AccountsItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AccountsItem>> PostAccountsItem(AccountsItem accountsItem)
        {
            _context.Accounts.Add(accountsItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAccountsItem", new { id = accountsItem.Id }, accountsItem);
        }

        // DELETE: api/AccountsItems/id
        [HttpDelete("{id}")]
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

        [HttpGet("check")]
        public async Task<ActionResult<bool>> AccountsItemExists(long id)
        {
            return await _context.Accounts.AnyAsync(e => e.Id == id);
        }
    }
}
