using UserService.Shared.DTOs;

namespace UserService.Core.Interfaces
{

    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> Exists(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task DeleteUserAsync(int id);
        Task<LoginDto> ValidateUserCredentialsAsync(string Email);
        Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto);
    }
}