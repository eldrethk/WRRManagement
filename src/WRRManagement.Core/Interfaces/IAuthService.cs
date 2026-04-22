using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.DTOs;

namespace WRRManagement.Core.Interfaces
{
    /// <summary>
    /// Authentication service — orchestrates login, registration,
    /// token refresh, and password management.
    ///
    /// This is the main service the AuthController calls.
    /// It coordinates between repositories, password service,
    /// and token service.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new staff user.
        /// Hashes password, creates user record, returns the new user's ID.
        /// </summary>
        Task<int> RegisterAsync(RegisterRequestDto request, string role, int? hotelId);

        /// <summary>
        /// Authenticates a user with email/password.
        /// Returns JWT access token + refresh token on success.
        /// Handles lockout logic on failed attempts.
        /// </summary>
        Task<TokenResponseDto> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Exchanges a valid refresh token for a new access + refresh token pair.
        /// Implements token rotation (old refresh token is revoked).
        /// </summary>
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// Revokes the provided refresh token (logout).
        /// </summary>
        Task LogoutAsync(string refreshToken);

        /// <summary>
        /// Changes the user's password.
        /// Verifies old password, hashes new one, revokes all refresh tokens.
        /// </summary>
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);

        /// <summary>
        /// Returns current user info from the database.
        /// </summary>
        Task<CurrentUserDto?> GetCurrentUserAsync(int userId);
    }
}
