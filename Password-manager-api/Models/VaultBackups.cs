using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;

namespace Password_manager_api.Models
{
    public class VaultBackups
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long UserId { get; set; }
        [Required]
        public long OwnerId { get; set; }
        [Required]
        public string EncryptedVaultBlob { get; set; }
        public DateTime BackupTimestamp { get; set; }

        [ForeignKey("")]
        public AccountsItem AccountsItem { get; set; }
    }
}
