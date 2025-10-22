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
    public class VaultBackupsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VaultBackupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/VaultBackups/UserId&OwnerId
        [HttpGet("{UserId}/{OwnerId}")]
        public async Task<ActionResult<VaultBackups>> GetVaultBackups(long userId, long ownerId)
        {
            var vaultBackups = await _context.VaultBackups.Where(item => item.UserId == userId && item.OwnerId == ownerId).ToListAsync();

            if (vaultBackups == null)
            {
                return NotFound();
            }

            return vaultBackups[0];
        }

        // POST: api/VaultBackups
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<VaultBackups>> PostVaultBackups(VaultBackups vaultBackups, long userId)
        {
            _context.VaultBackups.Add(vaultBackups);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVaultBackups", new { id = vaultBackups.Id }, vaultBackups);
        }

        // DELETE: api/VaultBackups/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVaultBackups(long id)
        {
            var vaultBackups = await _context.VaultBackups.FindAsync(id);
            if (vaultBackups == null)
            {
                return NotFound();
            }

            _context.VaultBackups.Remove(vaultBackups);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VaultBackupsExists(long id)
        {
            return _context.VaultBackups.Any(e => e.Id == id);
        }
    }
}
