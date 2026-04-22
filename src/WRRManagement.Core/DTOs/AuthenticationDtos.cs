using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.DTOs
{
    /// <summary>
    /// Login request DTO - Credentials for local authentication.
    ///
    /// SECURITY:
    /// - Email used as identifier (not username) - harder to guess
    /// - Password should be validated but never logged
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;
    }

    public class TokenResponseDto
    {
        /// <summary>
        /// The JWT access token.
        /// Include in Authorization header: "Bearer {token}"
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type - Always "Bearer" for JWT.
        /// </summary>
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Seconds until the access token expires.
        /// Client should refresh before this time.
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Refresh token for obtaining new access tokens.
        ///
        /// SECURITY:
        /// - Store securely (not in localStorage for web apps)
        /// - Can be revoked server-side if compromised
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Exact expiration time in UTC.
        /// Alternative to ExpiresIn for clients that prefer absolute time.
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// WHEN TO USE:
    /// - Access token is expired or about to expire
    /// - User is still "logged in" from their perspective
    /// </summary>
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }
    /// PASSWORD REQUIREMENTS (adjust to your security policy):
    /// - Minimum 8 characters
    /// - Recommend: upper, lower, number, special character
    /// </summary>
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username must be between 4 and 100 characters")]
        [MinLength(4, ErrorMessage ="Username must be at least 4 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores, dots, and hyphens")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, and number")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Display name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Change password request — requires old password for verification.
    /// </summary>
    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, and number")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Update user status — used by Admin/Manager to lock/unlock/ban users.
    /// </summary>
    public class UpdateUserStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public int Status { get; set; }
    }

    /// <summary>
    /// Update user role — Admin only.
    /// </summary>
    public class UpdateUserRoleDto
    {
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;

        /// <summary>Required for Manager/FrontDesk. Null for Admin.</summary>
        public int? HotelId { get; set; }
    }

    /// <summary>
    /// User list item — returned by user management endpoints.
    /// Does NOT include sensitive fields (PasswordHash, ApiKey).
    /// </summary>
    public class UserListItemDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? HotelId { get; set; }
        public string? HotelName { get; set; }
        public int Status { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// IMPORTANT:
    /// - This is the ONLY time the full API key is shown
    /// - User must save it securely
    /// - We only store the hash, not the key itself
    /// </summary>
    public class ApiKeyResponseDto
    {
        /// <summary>
        /// The generated API key.
        /// Format: "sk_live_xxxxxxxxxxxxx" or similar prefix.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// When the key was generated.
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Important message for the user.
        /// </summary>
        public string Message { get; set; } = "Save this API key securely. It will not be shown again.";
    }

    /// <summary>
    /// Current user info DTO - Information about the authenticated user.
    ///
    /// USE CASE:
    /// - Display user info in UI
    /// - Check remaining quota
    /// - Show user's role/permissions
    /// </summary>
    public class CurrentUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? HotelId { get; set; }      
        public DateTime? LastLoginDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public bool IsPasswordExpired { get; set; }
        public bool HasApiKey { get; set; }
    }
}
