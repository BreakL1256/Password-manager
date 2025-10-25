using Microsoft.EntityFrameworkCore;

namespace Password_manager_api.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AccountsItem> Accounts { get; set; }

        public DbSet<VaultBackups> VaultBackups { get; set; }
    }
}
