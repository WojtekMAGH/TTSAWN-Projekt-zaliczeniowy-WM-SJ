using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.Exceptions;
using UserService.Core.Interfaces;
using UserService.Shared.DTOs;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userRepository)
        {
            _userService = userRepository;
        }

        // Public endpoints
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExistsByEmail([FromQuery] string email)
        {
            bool exists = await _userService.Exists(email);
            return Ok(exists);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateUserDto createUserDto)
        {
            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email)
        {
            try
            {
                var loginDto = await _userService.ValidateUserCredentialsAsync(email);
                return Ok(new { userId = loginDto.Id, passwordHash = loginDto.PasswordHash });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // Protected endpoints
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var userDto = await _userService.GetUserByIdAsync(id);
            if (userDto == null)
            {
                return NotFound();
            }
            return Ok(userDto);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var createdUser = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
                {
                    return Unauthorized("User ID not found or invalid in token");
                }

                if (parsedUserId != id)
                {
                    return Forbid();
                }

                updateUserDto.Id = id;
                var updatedUser = await _userService.UpdateUserAsync(updateUserDto);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DuplicateEmailException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex}");
                return StatusCode(500, new { message = "An error occurred while updating the user" });
            }
        }
    }
}