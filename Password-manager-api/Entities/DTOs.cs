namespace Password_manager_api.Entities
{
    public class DTOs
    {
    }
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class VaultBackupDTO
    {
        public long VaultID { get; set; }
        public string EncryptedVaultBlob { get; set; }
    }
}
