using PostService_module.Core.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using PostService_module.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace PostService_module.Core.Services
{
    public class UserClientService : IUserClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserClientService> _logger;
        private readonly IConfiguration _configuration;

        public UserClientService(HttpClient httpClient, ILogger<UserClientService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<UserDTO> GetUserByIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Fetching user details for ID: {userId}");

                // Authorization header
                var token = _configuration["Jwt:Token"]; 
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"api/user/{userId}");
                _logger.LogInformation($"Response status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDTO>();
                    if (user != null)
                    {
                        _logger.LogInformation($"Successfully fetched user: {user.FirstName} {user.LastName}");
                        return user;
                    }
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error response from UserService: {error}");
                throw new Exception($"Failed to fetch user with ID {userId}. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in GetUserByIdAsync: {ex.Message}");
                throw;
            }
        }
    }
}
