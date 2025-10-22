using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Password_manager_api.Models
{
    public class AccountsItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [JsonIgnore]
        [Required]
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VaultBackups> VaultBackups { get; set; }
    }
}
