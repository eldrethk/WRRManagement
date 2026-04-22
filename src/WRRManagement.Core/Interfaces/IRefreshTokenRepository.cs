using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Infrastructure.Repositories
{
    public interface IRefreshTokenRepository
    {
        /// <summary>Stores a new refresh token (hashed).</summary>
        Task<int> CreateAsync(RefreshToken token);

        /// <summary>Retrieves a refresh token by its hashed value.</summary>
        Task<RefreshToken?> GetByTokenAsync(string tokenHash);

        /// <summary>Revokes a token, optionally linking to its replacement.</summary>
        Task RevokeAsync(string tokenHash, string? replacedByTokenHash = null);

        /// <summary>Revokes ALL tokens for a user (logout all devices, or theft detected).</summary>
        Task RevokeAllForUserAsync(int userId);
    }
}
