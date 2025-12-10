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
using Microsoft.Extensions.Configuration.UserSecrets;
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
        private readonly ILogger<VaultBackupsController> _logger;

        public VaultBackupsController(AppDbContext context, JWTService jwtService, ILogger<VaultBackupsController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        // Restores vault
        // GET: api/VaultBackups/VaultOwnerId
        [HttpGet("{VaultOwnerId}")]
        public async Task<ActionResult<VaultBackups>> GetVaultBackups(string vaultOwnerId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!long.TryParse(userIdClaim, out long userId))
            {
                _logger.LogError("Failed to parse user ID");
                return BadRequest(new { error = "Couldn't parse user ID" });
            }

            var storedVaultBackup = await _context.VaultBackups.Where(item => item.UserId == userId && item.VaultOwnerId == vaultOwnerId).FirstOrDefaultAsync();

            if (storedVaultBackup == null)
            {
                _logger.LogError("Backup couldn't be found:\n user ID: {userId}, vault ID: {vaultId}", userId, vaultOwnerId);
                return NotFound();
            }

            _logger.LogInformation("Vault backup restored succesfully: {blob}", storedVaultBackup.EncryptedVaultBlob);
            return Ok(new
            {
                encryptedvaultblob = storedVaultBackup.EncryptedVaultBlob,
            });
        }

        // POST: api/VaultBackups
        //Initiates new vault that hasn't yet existed
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
                VaultOwnerId = vaultBackup.VaultOwnerId,
            };

            _context.VaultBackups.Add(newBackup);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vault backup created succesfully");
            return Ok();
        }

        //Updates already existing vault
        // PUT: api/VaultBackups
        [HttpPut("{VaultOwnerId}")]
        public async Task<ActionResult> UpdateVaultBackups([FromBody] VaultBackupDTO vaultBackup,[FromRoute] string vaultOwnerId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!long.TryParse(userIdClaim, out long userId))
            {
                _logger.LogError("Failed to parse user ID");
                return BadRequest(new { error = "Couldn't parse user ID"});
            }

            var storedVaultBackup = await _context.VaultBackups.Where(item => item.UserId == userId && item.VaultOwnerId == vaultOwnerId).FirstOrDefaultAsync();

            if(storedVaultBackup == null)
            {
                _logger.LogError("Failed to find stored vault backup");
                NotFound();
            }

            storedVaultBackup.EncryptedVaultBlob = vaultBackup.EncryptedVaultBlob;
            storedVaultBackup.BackupTimestamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Vault backup updated succesfully");
            return Ok();

        }

        // DELETE: api/VaultBackups/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteVaultBackups(long id)
        //{
        //    var vaultBackups = await _context.VaultBackups.FindAsync(id);
        //    if (vaultBackups == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.VaultBackups.Remove(vaultBackups);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //public bool VaultBackupsExists(long id)
        //{
        //    return _context.VaultBackups.Any(e => e.Id == id);
        //}
    }
}
