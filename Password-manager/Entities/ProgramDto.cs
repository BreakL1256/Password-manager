using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_manager.Entities
{

    [Table("Accounts")]
    public class ProgramDto
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Category { get; set; }

        // Trash features
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }

    public class UserAccounts
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string KEKSalt { get; set; }
        public string EncryptedDEK { get; set; }
        public string UserIdentifier { get; set; }

        public bool CloudLinked { get; set; } = false;
        public long? CloudAccountId { get; set; }
        public string? CloudEmail { get; set; }
        public string? CloudPassword { get; set; }
        public string? CloudTokenEncrypted { get; set; }
        public DateTime? CloudTokenExpiry { get; set; }
        public DateTime? LastCloudSync { get; set; }
    }

    public class Notes
    {
        [PrimaryKey]
        [AutoIncrement]
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}