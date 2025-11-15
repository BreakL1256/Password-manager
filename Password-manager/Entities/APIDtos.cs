using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Password_manager.Entities
{
    internal class APIDtos
    {
    }
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserIdentifier { get; set; }
    }

    public class VaultBackupDTO
    {
        public string EncryptedVaultBlob { get; set; }
        public string VaultOwnerId { get; set; }
    }
}
