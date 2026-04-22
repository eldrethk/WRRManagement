using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Constants;
using WRRManagement.Core.DTOs;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Repositories;

namespace WRRManagement.Application.Services;

/// <summary>
/// AUTHENTICATION SERVICE
/// 
/// The central orchestrator for all authentication operations.
/// Controllers call this service — it coordinates between:
///   - IApiUserRepository (user data)
///   - IRefreshTokenRepository (token storage)
///   - IPasswordService (BCrypt hashing/verification)
///   - ITokenService (JWT + refresh token generation)
///
/// FLOW OVERVIEW:
/// ┌──────────┐     ┌─────────────┐     ┌──────────────────┐     ┌────────┐
/// │Controller │────>│ AuthService  │────>│ Repositories     │────>│Database│
/// │          │     │             │────>│ PasswordService  │     │        │
/// │          │     │             │────>│ TokenService     │     │        │
/// └──────────┘     └─────────────┘     └──────────────────┘     └────────┘
/// </summary>

public class AuthServices : IAuthService
{
    private readonly IApiUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthServices(IApiUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IPasswordService passwordService, ITokenService tokenService, IConfiguration configuraton)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _configuration = configuraton;
    }
      

    /// <summary>
    /// REGISTER A NEW STAFF USER
    /// 
    /// FLOW:
    /// 1. Check if email/username already exists
    /// 2. Hash the password with BCrypt
    /// 3. Create the ApiUser entity (validates role + hotelId)
    /// 4. Save to database
    /// 5. Return the new user's ID
    ///
    /// CALLED BY: AuthController.Register (Admin only)
    /// 
    /// NOTE: Only Admin can register new users. There is no
    /// self-registration for staff — Admin creates accounts
    /// and provides credentials to the employee.
    /// </summary>
    public async Task<int> RegisterAsync(RegisterRequestDto request, string role, int? hotelId)
    {
        //check for duplicate email
        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
            throw new InvalidOperationException("A user with that email already exist");

        //check for duplicate username
        var existingByUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingByUsername != null)
            throw new InvalidOperationException("A user with this username already exists");

        //Hash the password with BCrypt
        var passwordHash = _passwordService.HashPassword(request.Password);

        //create the entity
        //Admin cannot have hotelid - Manager and below must have hotelid
        var user = ApiUser.CreateWithPassword(
            username: request.Username,
            email: request.Email,
            passwordHash: passwordHash,
            displayName: request.DisplayName,
            role: role,
            hotelId: hotelId ?? 0,
            customClaims: null);

        var userId = await _userRepository.CreateAsync(user);

        Log.Information("New user registered {username} with role {role} for Hotel {hotelId}",
            request.Username, role, hotelId);
        return userId;
    }

    /// <summary>
    /// AUTHENTICATE A USER (LOGIN)
    /// 
    /// FLOW:
    /// 1. Look up user by email
    /// 2. Check account status (Active? Not banned/closed?)
    /// 3. Check lockout (too many failed attempts?)
    /// 4. Verify password with BCrypt
    /// 5. On failure: increment failed count, maybe trigger lockout
    /// 6. On success: reset failed count, generate tokens, update last login
    /// 7. Return access token + refresh token
    ///
    /// SECURITY:
    /// - Generic error message on failure ("Invalid credentials")
    ///   to prevent username/email enumeration
    /// - Lockout after N failed attempts (configurable)
    /// - Lockout duration is configurable (default 30 min)
    /// </summary>
    /// 
    public async Task<TokenResponseDto> LoginAsync(LoginRequestDto request)
    {
        //find the user
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            //don't reveal the email exist
            Log.Warning("Login failed: Email not found {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid credenitals");
        }

        //check account status
        if (!user.CanLogin())
        {
            Log.Warning("Login Failed: account status {status} for user {username}", user.Status, user.Username);
            var statusMsg = user.Status switch
            {
                UserAuthStatus.Pending => "Account is pending",
                UserAuthStatus.Closed => "Account is closed",
                UserAuthStatus.Banned => "Account has been banned",
                UserAuthStatus.LockedOut => "Account is locked out",
                _ => "Account is not active"
            };
            throw new UnauthorizedAccessException(statusMsg);
        }

        //check lockout
        if (user.IsLockedOut())
        {
            Log.Warning("Login Failed: account locked until {lockoutDate} for user {Username}", user.LockoutEnd, user.Username);
            throw new UnauthorizedAccessException($"Account is locked. Try again after {user.LockoutEnd:g}");

        }

        //vertify password
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            //password wrong -increment failed count
            var failCount = await _userRepository.IncrementAccessFailedAsync(user.Id);

            var maxAttempts = GetMaxFailedAttempts();

            if (failCount >= maxAttempts)
            {
                //lock account
                var lockoutMinutes = GetLockoutDurationMinutes();
                var lockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);

                await _userRepository.UpdateLockoutAsync(user.Id, lockoutEnd);

                Log.Warning("Account locked: {username} after {failedCount}. Try again at {lockedout}", user.Username, failCount, lockoutEnd);
                throw new UnauthorizedAccessException($"Account locked due to {maxAttempts} failed attempts");

            }
            Log.Warning("Login failed: invalid password for {username}.Attempt {failedcount} times. Max Attempts is {maxattempted}",
                user.Username, failCount, maxAttempts);

            throw new UnauthorizedAccessException("Invalid credentials");
        }

        //Success - generate token
        await _userRepository.ResetAccessFailedAsync(user.Id);
        if (user.LockoutEnd.HasValue)
        {
            await _userRepository.UpdateLockoutAsync(user.Id, null);
        }

        //upd last login timestamp
        await _userRepository.UpdateLastLoginAsync(user.Id);

        //Generate JWT access Token (contains user claims)
        var accesstoken = _tokenService.GenerateAccessToken(user);

        //Generate refresh token
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(refreshToken);

        //store the hashed refresh token in the db
        var refreshTokenExpireDays = GetRefreshTokenExpirationDays();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpireDays),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        Log.Information("user logged in: {username}", user.Username);

        //return tokens to the client
        var expirationMin = GetAccessTokenExpirationMinutes();
        return new TokenResponseDto
        {
            AccessToken = accesstoken,
            TokenType = "Bearer",
            ExpiresIn = expirationMin * 60, //convert to seconds for client
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMin)

        };

    }

    /// <summary>
    /// REFRESH AN ACCESS TOKEN
    /// 
    /// FLOW:
    /// 1. Hash the incoming refresh token
    /// 2. Look up the hash in the database
    /// 3. Validate: not revoked, not expired
    /// 4. If revoked token is reused → revoke ALL user tokens (theft detected!)
    /// 5. Revoke the old refresh token
    /// 6. Generate new access token + new refresh token
    /// 7. Store new refresh token hash in database
    /// 8. Return new token pair
    ///
    /// SECURITY — TOKEN ROTATION:
    /// Each refresh token can only be used ONCE. After use, it's revoked
    /// and a new one is issued. If an attacker steals a refresh token and
    /// the legitimate user also uses it, the system detects the reuse
    /// of a revoked token and invalidates ALL tokens for that user.
    /// </summary>
    /// 
    public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        //hash the incoming token to look it up in the db
        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var storeToken = await _refreshTokenRepository.GetByTokenAsync(tokenHash);

        if (storeToken == null)
        {
            Log.Warning("Refresh token not found");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // ============================================================
        // THEFT DETECTION: Revoked token reuse
        // ============================================================
        // If someone tries to use a revoked token, it means either:
        // 1. The attacker stole the old token and is trying to use it
        // 2. The legitimate user is using an old token after rotation
        //
        // Either way, revoke ALL tokens for this user as a precaution.
        // The user will need to log in again on all devices.
        // ============================================================
        if (storeToken.IsRevoked)
        {
            Log.Warning("Expired refresh token used for UserId {UserId}", storeToken.UserId);
            throw new UnauthorizedAccessException("Refresh token has expired. Please login again.");
        }

        // Get the user to generate new access token claims
        var user = await _userRepository.GetByIdAsync(storeToken.UserId);
        if (user == null || !user.CanLogin())
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(storeToken.UserId);
            throw new UnauthorizedAccessException("User account is not active");
        }

        //generate new token pair
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashToken(newRefreshToken);

        //revoke old refresh token
        await _refreshTokenRepository.RevokeAsync(tokenHash, newRefreshTokenHash);

        //store the new refresh token
        var refreshTokenExpireDay = GetRefreshTokenExpirationDays();
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpireDay),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

        Log.Debug("Token Refreshed for user {username}", user.Username);
        var expirationMinutes = GetAccessTokenExpirationMinutes();
        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            TokenType = "Bearer",
            ExpiresIn = expirationMinutes * 60,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    /// <summary>
    /// LOGOUT — Revoke the refresh token.
    /// 
    /// The access token will naturally expire (short-lived).
    /// We just revoke the refresh token so it can't be used
    /// to generate new access tokens.
    ///
    /// For "logout everywhere" functionality, use RevokeAllForUserAsync.
    /// </summary>
    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = _tokenService.HashToken(refreshToken);
        await _refreshTokenRepository.RevokeAsync(tokenHash);

        Log.Debug("User logged out (refresh token revoked)");
    }

    /// <summary>
    /// CHANGE PASSWORD
    /// 
    /// FLOW:
    /// 1. Get the user from database
    /// 2. Verify the current password
    /// 3. Hash the new password
    /// 4. Update in database (also resets LastPasswordChangedDate)
    /// 5. Revoke all refresh tokens (force re-login on all devices)
    ///
    /// WHY REVOKE ALL TOKENS?
    /// After a password change, all existing sessions should be
    /// invalidated. The user must log in again with the new password.
    /// This prevents an attacker who has a stolen token from
    /// maintaining access after the password is changed.
    /// </summary>
    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Verify current password
        if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash!))
            throw new UnauthorizedAccessException("Current password is incorrect");

        // Validate new password is different from current
        if (_passwordService.VerifyPassword(request.NewPassword, user.PasswordHash!))
            throw new InvalidOperationException("New password must be different from current password");

        // Hash and save new password
        var newHash = _passwordService.HashPassword(request.NewPassword);
        await _userRepository.UpdatePasswordAsync(userId, newHash);

        // Revoke all refresh tokens — force re-login on all devices
        await _refreshTokenRepository.RevokeAllForUserAsync(userId);

        Log.Information("Password changed for user {Username}", user.Username);
    }

    /// <summary>
    /// GET CURRENT USER INFO
    /// 
    /// Returns a DTO with the user's profile info.
    /// Called by GET /api/auth/me so the client can display
    /// user info and check password expiry status.
    /// </summary>
    public async Task<CurrentUserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        return new CurrentUserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role,
            HotelId = user.HotelId,
            LastLoginDate = user.LastLoginDate,
            LastPasswordChangedDate = user.LastPasswordChangedDate,
            IsPasswordExpired = user.IsPasswordExpired(GetPasswordExpiryDays()),
            HasApiKey = !string.IsNullOrEmpty(user.ApiKey)
        };
    }

    // ============================================================
    // CONFIGURATION HELPERS
    // ============================================================
    // Read values from appsettings.json "AuthSettings" section.
    // Fall back to AuthConstants.Defaults if not configured.
    // ============================================================

    private int GetMaxFailedAttempts()
        {
            return int.TryParse(_configuration["AuthSettings:MaxFailedAttempts"], out var val)
                ? val : AuthConstants.Defaults.MaxFailedAccessAttempts;
        }

        private int GetLockoutDurationMinutes()
        {
            return int.TryParse(_configuration["AuthSettings:LockoutDurationMinutes"], out var val)
                ? val : AuthConstants.Defaults.LockoutDurationMinutes;
        }

        private int GetAccessTokenExpirationMinutes()
        {
            return int.TryParse(_configuration["JwtSettings:AccessTokenExpirationMinutes"], out var val)
                ? val : AuthConstants.Defaults.AccessTokenExpirationMinutes;
        }

        private int GetRefreshTokenExpirationDays()
        {
            return int.TryParse(_configuration["AuthSettings:RefreshTokenExpirationDays"], out var val)
                ? val : AuthConstants.Defaults.RefreshTokenExpirationDays;
        }

        private int GetPasswordExpiryDays()
        {
            return int.TryParse(_configuration["AuthSettings:PasswordExpiryDays"], out var val)
                ? val : AuthConstants.Defaults.PasswordExpiryDays;
        }
}


