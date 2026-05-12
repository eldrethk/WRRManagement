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
    public class RackRateRepository : DapperRepository, IRackRateRepository
    {
        public RackRateRepository(IDbConnectionFactory dbConnectionFactory): base(dbConnectionFactory) { }

        public async Task<int> AddAsync(RackRate rackRate)
        {
            var parameters = new {
                Start = rackRate.StartDate, 
                End = rackRate.EndDate,
                RoomID = rackRate.RoomTypeID,
                TierA = rackRate.TierARate,
                TierB = rackRate.TierBRate,
                TierC = rackRate.TierCRate

            };
            return await ExecuteScalarIntAsync("dbo.genInsRackRate", parameters);
        }

        public async Task<IEnumerable<RackRate>> GetAllByRoomIdAsync(int roomId)
        {
            var parameters = new { RoomID = roomId };
            return await QueryAsync<RackRate>("dbo.genSelRackRateByRoomID", parameters);
        }

        public async Task<RackRate> GetByIdAsync(int rackRateId)
        {
            var parameters = new {RackRateID  = rackRateId};
            return await QueryFirstOrDefaultAsync<RackRate>("dbo.genSelRackRateByID", parameters);
        }

        public async Task<RackRate?> GetRateForDateAsync(int roomTypeId, DateTime date)
        {
            var parameters = new { RoomID = roomTypeId, Temp = date };
            return await QueryFirstOrDefaultAsync<RackRate?>("dbo.genSelRackRateByDate", parameters);
        }

        public async Task InvisibleAsync(int rackRateId)
        {
            var parameters = new { RackRateID = rackRateId };
            await ExecuteAsync("dbo.genInvisibleRackRate", parameters);
        }

        public async Task UpdateAsync(RackRate rackRate)
        {
            var parameters = new
            {
                RateID = rackRate.RackRateID,
                Start = rackRate.StartDate,
                End = rackRate.EndDate,
                RoomID = rackRate.RoomTypeID,
                TierA = rackRate.TierARate,
                TierB = rackRate.TierBRate,
                TierC = rackRate.TierCRate

            };
            await ExecuteAsync("dbo.genUpdRackRate", parameters);
        }

        public async Task VisibleAsync(int rackRateId)
        {
            var parameters = new { RackRateID = rackRateId };
            await ExecuteAsync("dbo.genVisibleRackRate", parameters);
        }
    }
}
