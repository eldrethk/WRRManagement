using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;

namespace WRRManagement.Infrastructure.Repositories
{
    public interface IApiUserRepository
    {
        /// <summary>Retrieves a user by email (primary login lookup).</summary>
        Task<ApiUser?> GetByEmailAsync(string email);

        /// <summary>Retrieves a user by ID.</summary>
        Task<ApiUser?> GetByIdAsync(int id);

        /// <summary>Retrieves a user by username (duplicate check during registration).</summary>
        Task<ApiUser?> GetByUsernameAsync(string username);

        /// <summary>Creates a new user. Returns the new user's ID.</summary>
        Task<int> CreateAsync(ApiUser user);

        /// <summary>Updates the user's password hash and LastPasswordChangedDate.</summary>
        Task UpdatePasswordAsync(int userId, string passwordHash);

        /// <summary>Updates the user's last login timestamp.</summary>
        Task UpdateLastLoginAsync(int userId);

        /// <summary>Updates the user's account status.</summary>
        Task UpdateStatusAsync(int userId, UserAuthStatus status);

        /// <summary>Increments failed access count. Returns new count.</summary>
        Task<int> IncrementAccessFailedAsync(int userId);

        /// <summary>Resets the failed access counter to zero.</summary>
        Task ResetAccessFailedAsync(int userId);

        /// <summary>Sets or clears the lockout end time.</summary>
        Task UpdateLockoutAsync(int userId, DateTime? lockoutEnd);

        /// <summary>Updates the user's role and hotel assignment.</summary>
        Task UpdateRoleAsync(int userId, string role, int? hotelId);

        /// <summary>Gets all users for a specific hotel.</summary>
        Task<IEnumerable<ApiUser>> GetUsersForHotelAsync(int hotelId);

        /// <summary>Gets all users across all hotels (Admin only).</summary>
        Task<IEnumerable<ApiUser>> GetAllUsersAsync();
    }
}
