using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SocialMediaBlazorApp.Models
{
    public class UploadResult
    {
        public string Url { get; set; } = string.Empty;
    }

    public class PostListDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public DateTime UserRegisteredAt { get; set; }
    }

    public class CreatePostDTO
    {
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; } = true;
    }

    public class UpdatePostDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }

    public class UserProfileDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
    }

    public class UpdateUserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        [CustomValidation(typeof(UpdateUserDto), nameof(ValidatePassword))]
        public string NewPassword { get; set; } = string.Empty;

        public static ValidationResult ValidatePassword(string password, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(password))
                return ValidationResult.Success;

            if (password.Length < 6)
                return new ValidationResult("If provided, password must be at least 6 characters");

            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$"))
                return new ValidationResult("Password must contain at least one uppercase letter, one lowercase letter, and one number");

            return ValidationResult.Success;
        }
    }
}

