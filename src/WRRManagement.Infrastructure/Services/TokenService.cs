using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Constants;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Infrastructure.Services
{
    /// <summary>
    /// JWT access token and refresh token generation.
    ///
    /// ACCESS TOKEN (JWT):
    /// - Contains user claims (who they are, what they can do)
    /// - Short-lived (default 60 min) — limits damage if stolen
    /// - Signed with HMAC-SHA256 using the secret key from appsettings
    /// - Stateless — server doesn't need to store it
    /// - Sent by client in Authorization header: "Bearer {token}"
    ///
    /// REFRESH TOKEN:
    /// - Cryptographically random string (not a JWT)
    /// - Long-lived (default 7 days)
    /// - Stored as SHA-256 hash in the database
    /// - Used to get a new access token without re-entering credentials
    /// - Supports "token rotation" — each use generates a new refresh token
    ///
    /// JWT STRUCTURE:
    /// Header:  { "alg": "HS256", "typ": "JWT" }
    /// Payload: { "sub": "42", "email": "user@hotel.com", "role": "Manager", ... }
    /// Signature: HMAC-SHA256(header + payload, secretKey)
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a JWT access token containing the user's claims.
        ///
        /// CLAIMS INCLUDED:
        /// - sub (Subject): User ID — unique identifier
        /// - email: User's email address
        /// - role: User's role (Admin, Manager, FrontDesk)
        /// - hotel_id: Hotel assignment (null for Admin)
        /// - display_name: Friendly name for UI display
        /// - pwd_changed: Last password change (UTC ticks) — checked by middleware
        ///
        /// These claims are available in controllers via:
        ///   User.FindFirst(ClaimTypes.NameIdentifier)?.Value  // User ID
        ///   User.FindFirst(ClaimTypes.Role)?.Value            // Role
        ///   User.IsInRole("Admin")                            // Role check
        /// </summary>
        public string GenerateAccessToken(ApiUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured");

            // ============================================================
            // SIGNING KEY
            // ============================================================
            // The secret key is used to sign the JWT.
            // Anyone with this key can create valid tokens — keep it secret!
            // Must be at least 256 bits (32 bytes) for HMAC-SHA256.
            // Stored in User Secrets (dev) or Azure Key Vault (prod).
            // ============================================================
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            // ============================================================
            // CLAIMS
            // ============================================================
            // Claims are key-value pairs embedded in the JWT payload.
            // They tell the server WHO the user is and WHAT they can do.
            //
            // Standard claims (from System.Security.Claims.ClaimTypes):
            //   - NameIdentifier = "sub" (subject / user ID)
            //   - Email = "email"
            //   - Role = "role" (used by [Authorize(Roles = "...")])
            //
            // Custom claims (from AuthConstants.Claims):
            //   - hotel_id, display_name, pwd_changed
            // ============================================================
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(AuthConstants.Claims.DisplayName, user.DisplayName),
                new Claim(AuthConstants.Claims.LastPasswordChanged,
                    user.LastPasswordChangedDate.Ticks.ToString())
            };

            // Only add HotelId claim if the user is hotel-scoped (not Admin)
            if (user.HotelId.HasValue)
            {
                claims.Add(new Claim(AuthConstants.Claims.HotelId, user.HotelId.Value.ToString()));
            }
            // ============================================================
            // TOKEN CREATION
            // ============================================================
            var expirationMinutes = int.TryParse(jwtSettings["AccessTokenExpirationMinutes"], out var mins)
                ? mins
                : AuthConstants.Defaults.AccessTokenExpirationMinutes;

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure random refresh token.
        ///
        /// Returns the PLAIN TEXT token to send to the client.
        /// You MUST hash it with HashToken() before storing in the database.
        ///
        /// WHY RANDOM BYTES INSTEAD OF GUID?
        /// - GUIDs are not cryptographically random (predictable patterns)
        /// - 64 bytes of random data = 512 bits of entropy
        /// - Base64 encoded for safe transport over HTTP
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Hashes a refresh token using SHA-256 for database storage.
        ///
        /// WHY HASH REFRESH TOKENS?
        /// - If the database is compromised, attacker can't use the hashes
        ///   to impersonate users (they need the original token)
        /// - Same principle as password hashing, but SHA-256 is fine here
        ///   because refresh tokens have high entropy (unlike passwords)
        ///
        /// NOTE: We use SHA-256 (not BCrypt) because:
        /// - Refresh tokens are random (not human-chosen like passwords)
        /// - They don't need slow hashing to prevent dictionary attacks
        /// - SHA-256 is fast and sufficient for high-entropy tokens
        /// </summary>
        public string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}
