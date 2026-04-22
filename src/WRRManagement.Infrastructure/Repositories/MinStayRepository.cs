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
    public class MinStayRepository : DapperRepository, IMinStayRepository
    {
        public MinStayRepository(IDbConnectionFactory dbConnectionFactory): base(dbConnectionFactory) { }
        public async Task<int> AddAsync(MinStay minStay)
        {
            var parameters = new
            {
                RoomID = minStay.RoomTypeID,
                Date = minStay.StayDate,
                Quantity = minStay.Quantity
            };
            return await ExecuteScalarIntAsync("dbo.genInsMinStay", parameters);
        }

        public async Task<IEnumerable<MinStay>> GetAllForRoomAsync(int roomId)
        {
            var parameters = new {RoomID = roomId };
            return await QueryAsync<MinStay>("dbo.genSelMinStayByRoomID", parameters);
        }

        public async Task<MinStay> GetByIdAsync(int minStayID)
        {
            var parameters = new {MinStayID = minStayID};
            return await QueryFirstOrDefaultAsync<MinStay>("dbo.genSelMinStayByID", parameters);
        }

        public async Task<int> GetQuantityForDateAsync(int roomId, DateTime date)
        {
            var parameters = new 
            {
                RoomID = roomId,
                PickedDate = date
            };
            return await ExecuteScalarIntAsync("dbo.genSelMinStayByDate", parameters);
        }

        public async Task UpdateAsync(int Quantity, int minStayId)
        {
            var parameters = new 
            {
                Quantity = Quantity,
                MinStayID = minStayId
            };
            await ExecuteAsync("dbo.genUpdMinStay", parameters);
        }
    }
}
