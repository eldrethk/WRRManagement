using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    /// <summary>
    /// JWT access token and refresh token generation.
    ///
    /// ACCESS TOKEN:
    /// - Short-lived (default 60 min)
    /// - Contains user claims (id, email, role, hotelId)
    /// - Sent in Authorization header: "Bearer {token}"
    ///
    /// REFRESH TOKEN:
    /// - Long-lived (default 7 days)
    /// - Cryptographically random string
    /// - Stored as SHA-256 hash in the database
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token with the user's claims.
        /// </summary>
        string GenerateAccessToken(ApiUser user);

        /// <summary>
        /// Generates a cryptographically secure random refresh token.
        /// Returns the plain text token (to send to client).
        /// The caller is responsible for hashing before storage.
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Hashes a refresh token using SHA-256 for secure storage.
        /// </summary>
        string HashToken(string token);
    }
}
