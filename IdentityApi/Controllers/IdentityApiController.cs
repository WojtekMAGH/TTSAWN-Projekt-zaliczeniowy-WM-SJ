using Microsoft.AspNetCore.Mvc;
using IdentityApi.Services;
using UserService.Shared.DTOs;
using IdentityApi.Core.DTOs;
using IdentityApi.Core.Interfaces;
using IdentityApi.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public UserController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        // POST: api/user/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(registerModel.Email) || string.IsNullOrWhiteSpace(registerModel.Password))
                {
                    return BadRequest(new { message = "Email and password are required." });
                }

                if (!new EmailAddressAttribute().IsValid(registerModel.Email))
                {
                    return BadRequest(new { message = "Invalid email format." });
                }

                // Register user
                CreateUserDto createUserDto = await _identityService.RegisterUserAsync(registerModel);
                return Ok(createUserDto);
            }
            catch (InvalidOperationException ex)
            {
                // Return 409 Conflict (duplicate email)
                return Conflict(new { message = "User already exists", details = ex.Message });
            }
            catch (Exception ex)
            {
                // General error
                return StatusCode(500, new { message = "An error occurred during registration", details = ex.Message });
            }
        }

        // POST: api/user/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                // Validatadtion
                if (string.IsNullOrWhiteSpace(loginModel.Email) || string.IsNullOrWhiteSpace(loginModel.Password))
                {
                    return BadRequest(new { message = "Email and password are required." });
                }

                if (!new EmailAddressAttribute().IsValid(loginModel.Email))
                {
                    return BadRequest(new { message = "Invalid email format." });
                }

                // Token
                var token = await _identityService.AuthenticateUserAsync(loginModel);

                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Invalid credentials - notificaiton
                return Unauthorized(new { message = "Invalid credentials", details = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                // External API errors - 503
                return StatusCode(503, new { message = "Service unavailable, please try again later", details = ex.Message });
            }
            catch (Exception ex)
            {
                // General error 
                return StatusCode(500, new { message = "An unexpected error occurred during login", details = ex.Message });
            }
        }
    }
}
