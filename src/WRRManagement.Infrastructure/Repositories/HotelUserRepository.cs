using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class HotelUserRepository : DapperRepository, IHotelUserRepository
    {
        public HotelUserRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        public async Task<int> AddUserToHotel(string userId, int hotelId)
        {
            var parameters = new {UserID = userId, HotelId = hotelId};
            return await ExecuteScalarIntAsync("dbo.genInsHotelUser", parameters);

        }

        public async Task<IEnumerable<Hotel>> GetHotelsForUser(string userId)
        {
            var parameters = new { UserID = userId};
            return await QueryAsync<Hotel>("dbo.genSelHotelsAssociatedUser", parameters);
        }

        public async Task<IEnumerable<HotelUser>> GetUserForHotel(int hotelId)
        {
            var parameters = new { HotelID = hotelId};
            return await QueryAsync<HotelUser>("dbo.genSelUserForHotel", parameters);
        }
    }
}
