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

    public class ResponseDTO
    {
        public long Id { get; set; }
        public string Email { get; set; }
    }
}
