using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Text.Json.Serialization;

namespace Password_manager_api.Models
{
    public class VaultBackups
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long UserId { get; set; }
        [JsonIgnore]
        [Required]
        public string EncryptedVaultBlob { get; set; }
        public DateTime BackupTimestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public AccountsItem AccountsItem { get; set; }
    }
}
