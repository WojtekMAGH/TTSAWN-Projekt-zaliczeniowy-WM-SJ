namespace IdentityApi.Core.Models
{
    //Response from UserApi
    public class VerifyUserResponse
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
    }
}
