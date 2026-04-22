using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WRRManagement.Core.Constants;
using WRRManagement.Core.Enums;

namespace WRRManagement.Core.Entities
{
    /// <summary>
    /// API USER ENTITY - Represents a user who can consume the API.
    /// ROLES:
    /// - Admin: Global access across all hotels (HotelId = null)
    /// - Manager: Hotel-scoped access, can manage FrontDesk users
    /// - FrontDesk: Hotel-scoped access, basic operations
    ///
    /// SECURITY:
    /// - Passwords hashed with BCrypt (handled by IPasswordService)
    /// - Account lockout after N failed attempts
    /// - Forced password change every 60 days
    /// </summary>
    public class ApiUser 
    {
        #region Constants

        public const int UsernameMinLength = 4;
        public const int UsernameMaxLength = 100;
        public const int EmailMaxLength = 256;
        public const int PasswordHashMaxLength = 500;
        public const int DisplayNameMaxLength = 100;
        public const int RoleMaxLength = 50;
        public const int ApiKeyMaxLength = 500;    

        #endregion


        public int Id { get; private set; }
        public string Username { get; private set; } = string.Empty; //initialize required properties
        public string Email { get; private set; } = string.Empty;

        /// <summary>
        /// Hashed password for local authentication.
        ///
        /// SECURITY:
        /// - Use BCrypt for hashing
        /// - Never store plain text passwords
        /// - Use constant-time comparison
        /// </summary>
        public string? PasswordHash { get; private set; }

        private string? _displayName;
        public string DisplayName
        {
            get => string.IsNullOrEmpty(_displayName) ? Username : _displayName;
            set => _displayName = value;
        }
        /// COMMON VALUES: "Admin", "Manager", "FrontDesk"
        public string Role { get; private set; } = "FrontDesk";

        /// <summary>
        /// The hotel this user belongs to.
        /// NULL for Admin (global access across all hotels).
        /// Required for Manager and FrontDesk roles.
        /// </summary>
        public int? HotelId { get; private set; }

        /// <summary>
        /// Account status — controls whether the user can log in.
        /// Pending = awaiting activation, Active = can login,
        /// LockedOut = too many failed attempts, Closed/Banned = permanently disabled
        /// </summary>
        public UserAuthStatus Status { get; private set; } = UserAuthStatus.Pending;

        /// <summary>
        /// Number of consecutive failed login attempts.
        /// Resets to 0 on successful login.
        /// Triggers lockout when threshold is reached.
        /// </summary>
        public int AccessFailedCount { get; private set; } = 0;

        /// <summary>
        /// UTC time when the lockout expires.
        /// NULL = not locked out. Past date = lockout expired (can login).
        /// </summary>
        public DateTime? LockoutEnd { get; private set; }

        /// <summary>
        /// Last time the password was changed.
        /// Compared against PasswordExpiryDays to enforce rotation policy.
        /// </summary>
        public DateTime LastPasswordChangedDate { get; private set; } = DateTime.UtcNow;


        /// <summary>
        /// API Key for this user - used for simple API authentication.
        ///
        /// SECURITY:
        /// - Generate using cryptographically secure random generator
        /// - Store hashed version, compare hashes on validation
        /// - Allow users to regenerate if compromised
        /// </summary>
        public string? ApiKey { get; private set; }

        /// When the API key was last regenerated.       
        public DateTime? ApiKeyGeneratedAt { get; private set; }

        /// <summary>
        /// Count of requests made today.
        /// Reset daily by a background job or on first request of new day.
        /// </summary>
        public int RequestsToday { get; private set; } = 0;

        /// Date of the last request - used to detect day rollover.
        public DateTime? LastRequestDate { get; private set; }

        /// Total lifetime API requests made by this user.
        /// Useful for analytics and identifying heavy users.     
        public long TotalRequests { get; private set; } = 0;

        /// <summary>
        /// JSON string of additional claims for this user.
        /// Flexible way to store custom claims without schema changes.
        ///
        /// EXAMPLE: {"department":"Engineering","level":"Senior"}
        /// </summary>
        public string? CustomClaims { get; private set; }

        // Audit
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; private set; }

        private ApiUser() { }

        public static ApiUser CreateWithPassword(
            string username,
            string email,
            string passwordHash,
            string? displayName,
            string role,           
            int hotelId,
            string? customClaims)
        {

            ValidateUser(username);
            ValidateEmail(email);
            ValidatePasswordHash(passwordHash);
            ValidateRole(role);
            ValidateRoleHotelId(role, hotelId);

            if (displayName == null)
            {
                displayName = username;
            }

            ValidateOptionalFields(displayName);
           

            return new ApiUser
            {
                Username = username.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                DisplayName = displayName.Trim(),
                Role = role.Trim(),  
                HotelId = hotelId,
                Status = UserAuthStatus.Active,
                LastPasswordChangedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,              
                CustomClaims = customClaims
            };

        }

        /// <summary>
        /// Records a successful login — updates timestamp and resets failed count.
        /// </summary>
        public void RecordLogin()
        {
            LastLoginDate = DateTime.UtcNow;
            AccessFailedCount = 0;
            LockoutEnd = null;
        }

        /// <summary>
        /// Increments the failed access counter.
        /// Returns the new count so the caller can check against the threshold.
        /// </summary>
        public int IncrementAccessFailed()
        {
            AccessFailedCount++;
            return AccessFailedCount;
        }

        /// <summary>Resets the failed access counter to zero.</summary>
        public void ResetAccessFailed()
        {
            AccessFailedCount = 0;
        }

        /// <summary>
        /// Locks the account until the specified time.
        /// </summary>
        public void Lockout(DateTime lockoutEnd)
        {
            LockoutEnd = lockoutEnd;
            Status = UserAuthStatus.LockedOut;
        }

        /// <summary>
        /// Checks if the account is currently locked out.
        /// A lockout is active if LockoutEnd is set and in the future.
        /// </summary>
        public bool IsLockedOut()
        {
            return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the password needs to be changed based on policy.
        /// </summary>
        /// <param name="expiryDays">Number of days before password expires (default: 60)</param>
        public bool IsPasswordExpired(int expiryDays = AuthConstants.Defaults.PasswordExpiryDays)
        {
            return (DateTime.UtcNow - LastPasswordChangedDate).TotalDays > expiryDays;
        }

        /// <summary>
        /// Checks if the account can log in — must be Active and not locked out.
        /// </summary>
        public bool CanLogin()
        {
            if (Status != UserAuthStatus.Active && Status != UserAuthStatus.LockedOut)
                return false;

            // If locked out but lockout has expired, allow login
            if (Status == UserAuthStatus.LockedOut && !IsLockedOut())
                return true;

            return Status == UserAuthStatus.Active;
        }

        public void UpdateDisplayName(
            string? displayName
          )
        {
            if (displayName != null && displayName.Length > DisplayNameMaxLength)
                throw new ArgumentException($"Display name cannot exceed {DisplayNameMaxLength} characters", nameof(displayName));

            DisplayName = displayName?.Trim();
         
        }

        public void UpdateEmail(string newEmail)
        {
            ValidateEmail(newEmail);
            Email = newEmail.Trim().ToLowerInvariant();
        }

        public void UpdatePassword(string newPasswordHash)
        {
            ValidatePasswordHash(newPasswordHash);
            PasswordHash = newPasswordHash;
            LastPasswordChangedDate = DateTime.UtcNow;
        }

        public void UpdateRole(string newRole, int? hotelId)
        {
            ValidateRole(newRole);
            ValidateRoleHotelId(newRole, hotelId);
            Role = newRole.Trim();
            HotelId = hotelId;
        }

        public void UpdateStatus(UserAuthStatus newStatus)
        {
            Status = newStatus;
        }
    
        public void UpdateCustomClaims(string? customClaimsJson)
        {
            CustomClaims = customClaimsJson;
        }

      

        #region API Validation  

        public void SetApiKey(string hashedApiKey)
        {
            if (string.IsNullOrWhiteSpace(hashedApiKey))
                throw new ArgumentException("API key hash is required", nameof(hashedApiKey));

            if (hashedApiKey.Length > ApiKeyMaxLength)
                throw new ArgumentException($"API key hash cannot exceed {ApiKeyMaxLength} characters", nameof(hashedApiKey));

            ApiKey = hashedApiKey;
            ApiKeyGeneratedAt = DateTime.UtcNow;
        }

        #endregion


        #region Validation Methods

        public static void ValidateRoleHotelId(string role, int? hotelId)
        {
            if (role.Trim().Equals(AuthConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                if (hotelId.HasValue)
                    throw new ArgumentException("Admin role should not have a HotelId (global access)", nameof(hotelId));
            }
            else
            {
                if (!hotelId.HasValue)
                    throw new ArgumentException($"{role} role requires a HotelId", nameof(hotelId));
            }
        }
        private static void ValidateUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required", nameof(username));

            if (username.Length < UsernameMinLength || username.Length > UsernameMaxLength)
                throw new ArgumentException(
                    $"Username must be between {UsernameMinLength} and {UsernameMaxLength} characters",
                    nameof(username));

            // Optional: Add regex validation for allowed characters
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]+$"))
                throw new ArgumentException(
                    "Username can only contain letters, numbers, underscores, dots, and hyphens",
                    nameof(username));
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));

            if (email.Length > EmailMaxLength)
                throw new ArgumentException(
                    $"Email cannot exceed {EmailMaxLength} characters",
                    nameof(email));

            // Basic email format validation
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email format", nameof(email));
        }

        private static void ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash is required", nameof(passwordHash));

            if (passwordHash.Length > PasswordHashMaxLength)
                throw new ArgumentException(
                    $"Password hash cannot exceed {PasswordHashMaxLength} characters",
                    nameof(passwordHash));
        }

        private static void ValidateRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role is required", nameof(role));

            if (role.Length > RoleMaxLength)
                throw new ArgumentException(
                    $"Role cannot exceed {RoleMaxLength} characters",
                    nameof(role));
        }



        private static void ValidateOptionalFields(
            string? displayName
          )
        {
            if (displayName != null && displayName.Length > DisplayNameMaxLength)
                throw new ArgumentException(
                    $"Display name cannot exceed {DisplayNameMaxLength} characters",
                    nameof(displayName));

        }

        #endregion 
    }
}
