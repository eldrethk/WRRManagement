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
    public class RoomAllocationRepository : DapperRepository, IRoomAllocation
    {
        public RoomAllocationRepository(IDbConnectionFactory dbConnectionFactory): base(dbConnectionFactory) { }

        public async Task<int> AddAsync(RoomAllocation allocation)
        {
            var parameters = new
            {
                RoomID = allocation.RoomTypeID,
                Date = allocation.AllocateDate,
                Quantity = allocation.Quantity
            };
            return await ExecuteScalarIntAsync("dbo.genInsAllocation", parameters);
        }

        public async Task<bool> AllocationIsValidAsync(int roomId, DateTime start, DateTime end)
        {
            var parameters = new
            {
                RoomId = roomId,
                Start = start,
                End = end
            };

            return await ExecuteScalarBoolAsync("dbo.genVerifyRoomAvailabilityForRoomAllocation", parameters);
        }

        public async Task<IEnumerable<RoomAllocation>> GetAllForRoomAsync(int roomId)
        {
            var parameters = new { RoomId = roomId };
            return await QueryAsync<RoomAllocation>("dbo.genSelAllocationByRoomID", parameters);
        }

        public async Task<RoomAllocation> GetByIdAsync(int allocationId)
        {
            var parameters = new { AllocationID = allocationId };
            return await QueryFirstOrDefaultAsync<RoomAllocation>("dbo.genSelAllocationByID", parameters);

        }

        public async Task<int> GetQuantityForDayAsync(int roomId, DateTime date)
        {
            var parameters = new { RoomID = roomId, PickedDate = date };
            return await ExecuteScalarIntAsync("dbo.genSelAllocationByDay", parameters);
        }

        public async Task UpdateQuantityAsync(int qty, int allocationId)
        {
            var parameters =new {AllocationID = allocationId, Quantity = qty};
            await ExecuteAsync("dbo.genUpdAllocation", parameters);
        }
    }
}
