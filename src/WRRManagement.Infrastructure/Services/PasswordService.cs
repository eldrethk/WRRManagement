using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Infrastructure.Services
{
    /// <summary>
    /// Password hashing and verification using BCrypt.
    ///
    /// WHY BCRYPT?
    /// - Automatically generates and embeds a unique salt per password
    ///   (no separate salt column needed in the database)
    /// - Has a configurable "work factor" that controls how slow hashing is
    ///   (slower = harder to brute force, default work factor = 11)
    /// - Uses constant-time comparison internally (prevents timing attacks)
    /// - Industry standard — same algorithm used by ASP.NET Identity under the hood
    ///
    /// HOW THE HASH WORKS:
    /// Input:  "MyPassword123"
    /// Output: "$2a$11$K4GhM0Oe1GxW6VzQ8pZrOeK..." (60 chars)
    ///          ^^^^ ^^
    ///          |    |-- work factor (2^11 = 2048 iterations)
    ///          |------- BCrypt algorithm version
    ///
    /// The salt is embedded in the hash string itself, so you only
    /// need to store the single hash value — no separate salt column.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        // ============================================================
        // WORK FACTOR
        // ============================================================
        // Controls the computational cost of hashing.
        // Each increment doubles the time.
        //
        //   10 = ~65ms   (minimum recommended)
        //   11 = ~130ms  (default, good balance)
        //   12 = ~260ms  (more secure, slightly slower login)
        //   13 = ~520ms  (high security environments)
        //
        // Increase over time as hardware gets faster.
        // ============================================================

        private const int WorkFactor = 11;


        /// <summary>
        /// Hashes a plain text password using BCrypt.
        /// 
        /// USAGE:
        ///   var hash = _passwordService.HashPassword("MyPassword123");
        ///   // Store hash in database
        ///
        /// NEVER log or expose the plain text password.
        /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }


        /// <summary>
        /// Verifies a plain text password against a stored BCrypt hash.
        ///
        /// USAGE:
        ///   bool isValid = _passwordService.VerifyPassword("MyPassword123", storedHash);
        ///
        /// HOW IT WORKS:
        /// 1. Extracts the salt from the stored hash
        /// 2. Hashes the input password with that same salt
        /// 3. Compares the result using constant-time comparison
        /// 4. Returns true if they match
        ///
        /// SECURITY:
        /// - Constant-time comparison prevents timing attacks
        ///   (attacker can't determine "how close" a guess was
        ///   by measuring response time)
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
