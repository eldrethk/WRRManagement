using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    /// <summary>
    /// REFRESH TOKEN ENTITY
    /// 
    /// Represents a stored refresh token for JWT token rotation.
    ///
    /// FLOW:
    /// 1. User logs in -> receives access token + refresh token
    /// 2. Access token expires (short-lived, e.g. 60 min)
    /// 3. Client sends refresh token to /api/auth/refresh
    /// 4. Server validates refresh token, issues NEW access + refresh token
    /// 5. Old refresh token is revoked (ReplacedByToken = new token hash)
    ///
    /// SECURITY:
    /// - Token stored as SHA-256 hash in DB (never plain text)
    /// - If a revoked token is reused, ALL user tokens are revoked
    ///   (indicates stolen token — "refresh token rotation" pattern)
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        /// <summary>SHA-256 hash of the token value</summary>
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>NULL if active. Set to UTC time when revoked.</summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>Hash of the token that replaced this one during rotation.</summary>
        public string? ReplacedByToken { get; set; }

        // ============================================================
        // COMPUTED PROPERTIES
        // ============================================================

        /// <summary>True if the token has passed its expiration time.</summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        /// <summary>True if the token has been explicitly revoked.</summary>
        public bool IsRevoked => RevokedAt.HasValue;

        /// <summary>True if the token can still be used (not expired AND not revoked).</summary>
        public bool IsActive => !IsExpired && !IsRevoked;
    }
}
