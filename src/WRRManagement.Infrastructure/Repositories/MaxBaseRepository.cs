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
    public class MaxBaseRepository : DapperRepository, IMaxBaseRepository
    {
        public MaxBaseRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }
        public async Task<int> AddAsync(MaxBase maxBase)
        {
            var parameters = new 
            {
                RoomID = maxBase.RoomTypeID,
                BaseCount = maxBase.BaseCount,
                MaxCount = maxBase.MaxBaseCount
            };
            return await ExecuteScalarIntAsync("dbo.genInsMaxBase", parameters);
        }

        public async Task<MaxBase> GetByRoomID(int roomId)
        {
            var parameters = new
            {
                RoomID = roomId
            };
            return await QueryFirstOrDefaultAsync<MaxBase>("dbo.genSelMaxBaseByID", parameters);
        }
    }
}
