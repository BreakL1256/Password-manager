using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using Password_manager_api.Entities;
using Password_manager_api.Models;

namespace Password_manager_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VaultBackupsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JWTService _jwtService;
        private readonly ILogger _logger;

        public VaultBackupsController(AppDbContext context, JWTService jwtService, ILogger logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        // GET: api/VaultBackups/UserId&Id
        [HttpGet("{userId}/{id}")]
        public async Task<ActionResult<VaultBackups>> GetVaultBackups(long id)
        {

            var vaultBackups = await _context.VaultBackups.Where(item => item.UserId == userId && item.Id == id).ToListAsync();

            if (vaultBackups == null)
            {
                return NotFound();
            }

            return vaultBackups[0];
        }

        // POST: api/VaultBackups
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> PostVaultBackups(VaultBackupDTO vaultBackup)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(userIdClaim, out long userId))
            {
                _logger.LogError("Failed to parse user ID");
                return BadRequest(new {error = "Couldn't parse user ID"});
            }

            var newBackup = new VaultBackups
            {
                UserId = userId,
                EncryptedVaultBlob = vaultBackup.EncryptedVaultBlob,
            };

            _context.VaultBackups.Add(newBackup);
            await _context.SaveChangesAsync();

            return Ok( new
            {
                VaultId = newBackup.Id
            });
        }

        // PUT: api/VaultBackups
        [HttpPut]
        public async Task<ActionResult> UpdateVaultBackups(VaultBackupDTO vaultBackup)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!long.TryParse(userIdClaim, out long userId))
            {
                _logger.LogError("Failed to parse user ID");
                return BadRequest(new { error = "Couldn't parse user ID"});
            }

            var storedVaultBackup = _context.VaultBackups.Where(item => item.UserId == userId && item.Id == vaultBackup.VaultID).FirstOrDefaultAsync();

            if(storedVaultBackup == null)
            {
                _logger.LogError("Failed to find stored vault backup");
                NotFound();
            }

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

        public bool VaultBackupsExists(long id)
        {
            return _context.VaultBackups.Any(e => e.Id == id);
        }
    }
}
