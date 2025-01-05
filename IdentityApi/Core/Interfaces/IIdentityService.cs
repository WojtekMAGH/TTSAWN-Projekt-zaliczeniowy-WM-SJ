using IdentityApi.Core.DTOs;
using IdentityApi.Core.Models;
using UserService.Shared.DTOs;

namespace IdentityApi.Core.Interfaces
{
    public interface IIdentityService
    {
        Task<CreateUserDto> RegisterUserAsync(RegisterModel registerModel);
        Task<bool> CheckIfUserExistsByEmailAsync(string email);
        Task<string> AuthenticateUserAsync(LoginModel loginModel);
    }
}
