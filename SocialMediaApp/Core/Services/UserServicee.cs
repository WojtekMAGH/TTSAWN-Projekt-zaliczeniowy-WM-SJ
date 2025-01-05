using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Core.Entities;
using UserService.Core.Exceptions;
using UserService.Core.Interfaces;
using UserService.Shared.DTOs;

namespace UserService.Core.Services;

public class UserServicee : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public UserServicee(
        IUserRepository userRepository,
        IMapper mapper,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Validate email - unique
        if (await _userRepository.EmailExistsAsync(createUserDto.Email))
        {
            throw new DuplicateEmailException(createUserDto.Email);
        }

        // Create new user
        var user = _mapper.Map<User>(createUserDto);

        // Save to database
        var createdUser = await _userRepository.AddAsync(user);

        // Return DTO
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            throw new UserNotFoundException(id);

        return _mapper.Map<UserDto>(user);
    }
    public async Task<bool> Exists(string email)
    {

        return await _userRepository.EmailExistsAsync(email);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> UpdateUserAsync(UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(updateUserDto.Id);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {updateUserDto.Id} not found");
        }

        if (!string.Equals(user.Email, updateUserDto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailExists = await _userRepository.EmailExistsAsync(updateUserDto.Email);
            if (emailExists)
            {
                throw new DuplicateEmailException(updateUserDto.Email);
            }
        }

        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.Email = updateUserDto.Email;

        if (!string.IsNullOrEmpty(updateUserDto.NewPassword))
        {
            user.HashPassword = _passwordHasher.HashPassword(user, updateUserDto.NewPassword);
        }

        await _userRepository.UpdateAsync(user);
        return _mapper.Map<UserDto>(user);
    }


    public async Task DeleteUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new UserNotFoundException(id);

        await _userRepository.DeleteAsync(user);
    }

    public async Task<LoginDto> ValidateUserCredentialsAsync(string email)
    {
        // Retrieve the user by email
        User user = await _userRepository.GetByEmailAsync(email);

        // If user doesn't exist, return an UnauthorizedAccess
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // If valid, map user data to LoginDto
        var loginDto = new LoginDto
        {
            Id = user.Id,
            PasswordHash = user.HashPassword
        };

        // Return the LoginDto
        return loginDto;
    }


    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FirstName),
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"])); // Secret from config
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(double.Parse(_configuration["Jwt:ExpiryInHours"])), // Expiry time from config
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}