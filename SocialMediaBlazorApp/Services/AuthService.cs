using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;

namespace SocialMediaBlazorApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager, IJSRuntime jsRuntime)
        {
            _httpClient = httpClientFactory.CreateClient("IdentityApi");
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrWhiteSpace(token); // Check if token exists
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            try
            {
                var loginModel = new LoginModel { Email = email, Password = password };

                // Validate the model before sending
                if (!loginModel.IsValid())
                {
                    throw new ValidationException("Invalid login data. Ensure all fields are correctly filled.");
                }

                var response = await _httpClient.PostAsJsonAsync("api/user/login", loginModel);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (responseData?.Token != null)
                    {
                        await SaveTokenAsync(responseData.Token);
                        _navigationManager.NavigateTo("/"); // Redirect to home page after successful login
                        return responseData.Token;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new InvalidOperationException("Invalid credentials. Please check your email and password.");
                }

                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Login failed: {errorMessage}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"An error occurred during login: {ex.Message}");
            }
        }

        public async Task<bool> RegisterAsync(string firstName, string lastName, string email, string password)
        {
            try
            {
                var registerModel = new RegisterModel
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = password
                };

                if (!registerModel.IsValid())
                {
                    throw new ValidationException("Invalid registration data. Ensure all fields are correctly filled.");
                }

                var response = await _httpClient.PostAsJsonAsync("api/user/register", registerModel);

                if (response.IsSuccessStatusCode)
                {
                    return true; // Registration successful
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // (duplicate email)
                    throw new InvalidOperationException("This email is already registered. Please use a different email.");
                }

                // Handle other errors
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Registration failed: {errorResponse}");
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"An error occurred while communicating with the server: {ex.Message}");
            }
        }


        private async Task SaveTokenAsync(string token)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        }

        public async Task<string?> GetTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            _navigationManager.NavigateTo("/login");
        }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public bool IsValid()
        {
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(this, context, results, true);
        }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public bool IsValid()
        {
            var context = new ValidationContext(this);
            var results = new List<ValidationResult>();
            return Validator.TryValidateObject(this, context, results, true);
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

}