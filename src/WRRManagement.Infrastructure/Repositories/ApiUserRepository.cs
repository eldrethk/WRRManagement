using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class ApiUserRepository :DapperRepository,  IApiUserRepository
    {
        public ApiUserRepository(IDbConnectionFactory connectionFactory) :base(connectionFactory) 
        {
            
        }

        public async Task<int> CreateAsync(ApiUser user)
        {
            var parameters = new {
              UserName = user.Username,
              Email = user.Email,
              PasswordHash = user.PasswordHash,
              DisplayName = user.DisplayName,
              Role = user.Role,
              HotelId = user.HotelId,
              Status = user.Status
            };

            return await ExecuteScalarIntAsync("dbo.authInsApiUser", parameters);
        }

        public async Task<IEnumerable<ApiUser>> GetAllUsersAsync()
        {
            IEnumerable<ApiUser> users = await QueryAsync<ApiUser>("dbo.authSelAllUsers", null);
            return users;
        }

        public async Task<ApiUser?> GetByEmailAsync(string email)
        {
            var parameters = new { Email = email };

            var apiUser = await QueryFirstOrDefaultAsync<ApiUser>("dbo.authSelApiUserByEmail", parameters);
            return apiUser;
        }

        public async Task<ApiUser?> GetByIdAsync(int id)
        {
            var parameters = new {Id = id};

            var apiUser = await QueryFirstOrDefaultAsync<ApiUser>("dbo.authSelApiUserById", parameters);
            return apiUser;
        }

        public async Task<ApiUser?> GetByUsernameAsync(string username)
        {
            var parameters = new {username = username};
            var apiUser = await QueryFirstOrDefaultAsync<ApiUser>("dbo.authSelApiUserByUsername", parameters);
            return apiUser;
        }

        public async Task<IEnumerable<ApiUser>> GetUsersForHotelAsync(int hotelId)
        {
            var parameters = new {HotelId = hotelId};
            IEnumerable<ApiUser> users = await QueryAsync<ApiUser>("dbo.authSelUsersForHotel", parameters);
            return users;
        }

        public async Task<int> IncrementAccessFailedAsync(int userId)
        {
            var parameters = new {Id = userId};
            return await ExecuteScalarIntAsync("dbo.authIncAccessFailed", parameters);
        }

        public async Task ResetAccessFailedAsync(int userId)
        {
            var parameters = new {Id = userId};
            await ExecuteAsync("dbo.authResetAccessFailed", parameters);
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var parameters = new {Id =  userId};
            await ExecuteAsync("dbo.authUpdLastLogin", parameters);
        }

        public async Task UpdateLockoutAsync(int userId, DateTime? lockoutEnd)
        {
            var parameters = new {Id = userId, LockoutEnd = lockoutEnd};
            await ExecuteAsync("dbo.authUpdLockout", parameters);
        }

        public async Task UpdatePasswordAsync(int userId, string passwordHash)
        {
            var parameters = new {Id = userId, PasswordHash = passwordHash};
            await ExecuteAsync("dbo.authUpdPassword", parameters);
        }

        public async Task UpdateRoleAsync(int userId, string role, int? hotelId)
        {
            var parameters = new {Id = userId, Role = role, HotelId = hotelId};
            await ExecuteAsync("dbo.authUpdUserRole", parameters);
        }

        public async Task UpdateStatusAsync(int userId, UserAuthStatus status)
        {
            var parameters = new {Id = userId, Status = status};
            await ExecuteAsync("dbo.authUpdUserStatus", parameters);
        }
    }
}
