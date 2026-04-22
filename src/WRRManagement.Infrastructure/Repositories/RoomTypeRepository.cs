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
    public class RoomTypeRepository :DapperRepository, IRoomTypeRepository
    {
        public RoomTypeRepository(IDbConnectionFactory dbConnectionFactory): base(dbConnectionFactory) { }

        public async Task<RoomType> GetByIdAsync(int roomTypeId)
        {
            var parameters = new { RoomID =  roomTypeId};       
            return await QueryFirstOrDefaultAsync<RoomType>("dbo.genSelRoomTypeByID", parameters);
        }

        public async Task<IEnumerable<RoomType>> GetAllForHotelAsync(int hotelID)
        {
            var parameters = new {HotelID  =  hotelID};
            return await QueryAsync<RoomType>("dbo.genSelRoomsByHotelID", parameters);
        }

        public async Task InvisibleAsync(int roomTypeId)
        {
            var parameters = new {RoomID = roomTypeId};
            await ExecuteAsync("dbo.genInvisibleRoomType", parameters);
        }

        public async Task<int> AddAsync(RoomType roomType)
        {
            var parameters = new
            {
                HotelID = roomType.HotelID,
                Name = roomType.Name,
                Desc = roomType.Description,
                Adult = roomType.AdultBase,
                Max = roomType.MaxBase,
                BedType = roomType.BedType,

            };

            return await ExecuteScalarIntAsync("dbo.genInsRoomType", parameters);
        }

        public async Task UpdateAsync(RoomType roomType)
        {

            var parameters = new
            {
               
                Name = roomType.Name,
                Desc = roomType.Description,
                Adult = roomType.AdultBase,
                Max = roomType.MaxBase,
                BedType = roomType.BedType,

            };

            await ExecuteAsync("dbo.genUpdRoomType", parameters);
        }
    }
}
