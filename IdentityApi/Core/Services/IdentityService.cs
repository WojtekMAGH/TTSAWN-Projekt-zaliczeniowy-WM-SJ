using System.Text;
using System.Security.Claims;
using IdentityApi.Core.Interfaces;
using IdentityApi.Core.DTOs;
using UserService.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using IdentityApi.Core.Models;

using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace IdentityApi.Services


{
    public class IdentityService : IIdentityService
    {
        private readonly HttpClient _httpClient;
        private readonly string _peopleApiUrl;
        private readonly string _jwtSecret;
        private readonly double _jwtExpiryInMinutes;
        private readonly IPasswordHasher<RegisterModel> _passwordHasher;
        private readonly IConfiguration _configuration;

        public IdentityService(HttpClient httpClient, IPasswordHasher<RegisterModel> passwordHasher, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _passwordHasher = passwordHasher;

            // Validate JWT configuration
            _jwtSecret = configuration["Jwt:Secret"]
                ?? throw new ArgumentNullException("Jwt:Secret is not configured.");

            _peopleApiUrl = configuration["PeopleApiUrl"]
                ?? throw new ArgumentNullException("PeopleApiUrl is not configured.");

            // Validate JWT configuration exists
            if (string.IsNullOrEmpty(configuration["Jwt:Issuer"]))
                throw new ArgumentNullException("Jwt:Issuer is not configured.");
            if (string.IsNullOrEmpty(configuration["Jwt:Audience"]))
                throw new ArgumentNullException("Jwt:Audience is not configured.");

            Console.WriteLine("IdentityService initialized with:");
            Console.WriteLine($"Issuer: {configuration["Jwt:Issuer"]}");
            Console.WriteLine($"Audience: {configuration["Jwt:Audience"]}");

            _jwtExpiryInMinutes = configuration.GetValue<double>("Jwt:ExpiryInHours", 24) * 60;
        }

        // Checking User by email in UserService
        public async Task<bool> CheckIfUserExistsByEmailAsync(string email)
        {
            var response = await _httpClient.GetAsync($"{_peopleApiUrl}/api/User?email={email}");
            if (response.IsSuccessStatusCode)
            {
                var exists = await response.Content.ReadFromJsonAsync<bool>();
                return exists;
            }
            return false;
        }

        // Register a new user
        public async Task<CreateUserDto> RegisterUserAsync(RegisterModel registerModel)
        {
            // Check if the user already exists
            var userExists = await CheckIfUserExistsByEmailAsync(registerModel.Email);
            if (userExists)
            {
                throw new InvalidOperationException("User already exists");
            }

            // Password hasher
            var hashedPassword = _passwordHasher.HashPassword(null, registerModel.Password);

            // Prepare the user data
            var userToCreate = new CreateUserDto
            {
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
                Email = registerModel.Email,
                HashPassword = hashedPassword
            };

            // Send a request to UserApi
            var response = await _httpClient.PostAsJsonAsync($"{_peopleApiUrl}/api/User/register", userToCreate);

          
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create user in PeopleApi: {errorMessage}");
            }
            
            var createdUser = await response.Content.ReadFromJsonAsync<CreateUserDto>();

            return createdUser;
        }

        // Authenticate user
        public async Task<string> AuthenticateUserAsync(LoginModel loginModel)
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(loginModel.Email) || string.IsNullOrWhiteSpace(loginModel.Password))
                {
                    throw new UnauthorizedAccessException("Email and password are required.");
                }

                // Call PeopleApi - credidentials data
                var response = await _httpClient.GetAsync($"{_peopleApiUrl}/api/User/login?email={loginModel.Email}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new UnauthorizedAccessException("Invalid credentials. User not found.");
                }

                var user = await response.Content.ReadFromJsonAsync<VerifyUserResponse>();
                if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Invalid credentials. User data is incomplete.");
                }

                // Verify hashed password
                var isValidPassword = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, loginModel.Password);
                if (isValidPassword != PasswordVerificationResult.Success)
                {
                    throw new UnauthorizedAccessException("Invalid credentials. Password does not match.");
                }

                // token
                return GenerateJwtToken(user.UserId);
            }
            catch (HttpRequestException ex)
            {
            
                Console.WriteLine($"HTTP request failed: {ex.Message}");
                throw new UnauthorizedAccessException("Authentication failed due to a server error.");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                throw new UnauthorizedAccessException("Authentication failed due to an unexpected error.");
            }
        }


        // JTW TOKEN - Authenticated User
        private string GenerateJwtToken(int userId)
        {
            Console.WriteLine($"Generating token for user {userId}");
            Console.WriteLine($"Using config Issuer: {_configuration["Jwt:Issuer"]}");
            Console.WriteLine($"Using config Audience: {_configuration["Jwt:Audience"]}");

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var secretKey = _configuration["Jwt:Secret"] ??
                throw new InvalidOperationException("JWT Secret not configured");
            var issuer = _configuration["Jwt:Issuer"] ??
                throw new InvalidOperationException("JWT Issuer not configured");
            var audience = _configuration["Jwt:Audience"] ??
                throw new InvalidOperationException("JWT Audience not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            var tokenString = handler.WriteToken(token);

            var decodedToken = handler.ReadJwtToken(tokenString);
            Console.WriteLine("Generated token details:");
            Console.WriteLine($"Issuer: {decodedToken.Issuer}");
            Console.WriteLine($"Claims: {string.Join(", ", decodedToken.Claims.Select(c => $"{c.Type}: {c.Value}"))}");
            Console.WriteLine($"Token Payload: {tokenString}");

            return tokenString;
        }
    }
}
