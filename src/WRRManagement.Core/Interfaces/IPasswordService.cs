using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Interfaces
{
    public interface IPasswordService
    {
        /// <summary>
        /// Hashes a plain text password using BCrypt.
        /// The returned hash includes the salt and algorithm metadata.
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a plain text password against a BCrypt hash.
        /// Uses constant-time comparison to prevent timing attacks.
        /// </summary>
        bool VerifyPassword(string password, string passwordHash);
    }
}
