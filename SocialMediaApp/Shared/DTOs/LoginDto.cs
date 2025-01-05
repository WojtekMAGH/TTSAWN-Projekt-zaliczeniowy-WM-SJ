namespace UserService.Shared.DTOs
{
    public class LoginDto
    {
        public int Id { get; set; } 
        public string PasswordHash { get; set; } = string.Empty;
    }
}
