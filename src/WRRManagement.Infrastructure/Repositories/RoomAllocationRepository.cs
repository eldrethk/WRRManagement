using Dapper;
using System.Data;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class RoomAllocationRepository : DapperRepository, IRoomAllocationRepository
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

        public async Task<int> AddAsync(int roomTypeID, DateTime date, int qty)
        {
            var parameters = new
            {
                RoomID = roomTypeID,
                Date = date,
                Quantity = qty
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

        public async Task AddDateRangeAsync(int RoomTypeId, DateTime start, DateTime end, int qty)
        {
            if(start > DateTime.MinValue && end > DateTime.MinValue)
            {
                DateTime temp = start;
                while(temp <= end)
                {
                    await AddAsync(RoomTypeId, temp, qty);
                    temp = temp.AddDays(1);
                }
            }
        }

        public async Task<int> LowestAllocationAsync(int roomTypeId, DateTime start, DateTime end)
        {
            var parameters = new DynamicParameters();
            parameters.Add("RoomID", roomTypeId);
            parameters.Add("Start", start);
            parameters.Add("End", end);
            parameters.Add("Qty", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync("dbo.genSelLowQtyForAllocation", parameters, commandType: System.Data.CommandType.StoredProcedure);
            return parameters.Get<int>("Qty");
        }
    }
}
