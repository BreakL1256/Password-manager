using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_manager.Entities {

    [Table("Accounts")]
    public class ProgramDto
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

    }

    public class UserAccounts
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string KEKSalt { get; set; }
        public string EncryptedDEK { get; set; }
    }
}
