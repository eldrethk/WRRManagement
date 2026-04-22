using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Constants
{
    /// <summary>
    /// Centralized authentication and authorization constants.
    /// 
    /// WHY CONSTANTS?
    /// - Avoids magic strings scattered across the codebase
    /// - Single source of truth for role names, policy names, and settings
    /// - Compile-time safety — typos are caught immediately
    /// - Used by: Controllers ([Authorize(Policy = ...)]), AuthService, TokenService
    /// </summary>
    public class AuthConstants
    {
        // ============================================================
        // ROLE NAMES
        // ============================================================
        // These must match the Role values stored in the ApiUsers table.
        // Used in JWT claims and authorization policies.
        // ============================================================
        public static class Roles
        {
            public const string Admin = "Admin";
            public const string Manager = "Manager";
            public const string FrontDesk = "FrontDesk";
        }

        // ============================================================
        // AUTHORIZATION POLICY NAMES
        // ============================================================
        // Referenced in controllers: [Authorize(Policy = Policies.RequireAdmin)]
        // Defined in DICommonExtensions.AddAuthorizationServices()
        // ============================================================
        public static class Policies
        {
            /// <summary>Only Admin role</summary>
            public const string RequireAdmin = "RequireAdmin";

            /// <summary>Admin or Manager</summary>
            public const string RequireManagerOrAbove = "RequireManagerOrAbove";

            /// <summary>Admin, Manager, or FrontDesk (any authenticated staff)</summary>
            public const string RequireStaff = "RequireStaff";
        }

        // ============================================================
        // JWT CLAIM TYPES
        // ============================================================
        // Custom claim names embedded in the JWT token.
        // Standard claims (sub, email) use ClaimTypes from System.Security.
        // These are for WRR-specific data.
        // ============================================================
        public static class Claims
        {
            /// <summary>The user's HotelId — NULL for Admin (global access)</summary>
            public const string HotelId = "hotel_id";

            /// <summary>Display name shown in UI</summary>
            public const string DisplayName = "display_name";

            /// <summary>
            /// Last password change date — checked by PasswordExpiryMiddleware.
            /// Stored as UTC ticks in the JWT to avoid date format issues.
            /// </summary>
            public const string LastPasswordChanged = "pwd_changed";
        }

        // ============================================================
        // SECURITY SETTINGS (defaults — can be overridden in appsettings.json)
        // ============================================================
        public static class Defaults
        {
            /// <summary>Days before a password must be changed</summary>
            public const int PasswordExpiryDays = 60;

            /// <summary>Failed login attempts before account lockout</summary>
            public const int MaxFailedAccessAttempts = 5;

            /// <summary>Minutes the account stays locked after max failed attempts</summary>
            public const int LockoutDurationMinutes = 30;

            /// <summary>Access token lifetime in minutes (short-lived for security)</summary>
            public const int AccessTokenExpirationMinutes = 60;

            /// <summary>Refresh token lifetime in days (longer-lived, stored in DB)</summary>
            public const int RefreshTokenExpirationDays = 7;
        }
    }
}
