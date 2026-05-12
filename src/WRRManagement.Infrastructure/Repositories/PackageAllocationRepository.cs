using Dapper;
using System.Data;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class PackageAllocationRepository : DapperRepository, IPackageAllocationRepository
    {
        public PackageAllocationRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<PackageAllocation?> GetByIdAsync(int allocationId)
        {
            var parameters = new { AllocationID = allocationId };
            return await QueryFirstOrDefaultAsync<PackageAllocation>("dbo.genSelPackageAllocationByID", parameters);
        }

        public async Task<IEnumerable<PackageAllocation>> GetForPackageRoomAsync(int packageId, int roomTypeId)
        {
            var parameters = new { PackageID = packageId, RoomTypeID = roomTypeId };
            return await QueryAsync<PackageAllocation>("dbo.genSelPackageAllocationByRoomID", parameters);
        }

        public async Task<IEnumerable<Package>> GetPackagesWithAllocationsAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<Package>("dbo.genSelPackagesWithAllocation", parameters);
        }

        public async Task<int> AddAsync(int roomTypeId, int packageId, DateTime date, int quantity)
        {
            var parameters = new
            {
                RoomTypeID = roomTypeId,
                PackageID = packageId,
                AllocateDate = date,
                Quantity = quantity
            };
            return await ExecuteScalarIntAsync("dbo.genInsPackageAllocation", parameters);
        }

        public async Task AddDateRangeAsync(int roomTypeId, int packageId, DateTime start, DateTime end, int quantity)
        {
            for (var date = start; date < end; date = date.AddDays(1))
            {
                await AddAsync(roomTypeId, packageId, date, quantity);
            }
        }

        public async Task UpdateAsync(int quantity, int allocationId)
        {
            var parameters = new { AllocationID = allocationId, Quantity = quantity };
            await ExecuteAsync("dbo.genUpdPackageAllocation", parameters);
        }

        public async Task RollbackAsync(int roomTypeId, int packageId, DateTime date)
        {
            var parameters = new { RoomTypeID = roomTypeId, PackageID = packageId, Date = date };
            await ExecuteAsync("dbo.genRollBackPackageAllocation", parameters);
        }

        public async Task<bool> IsValidAsync(int roomTypeId, int packageId, DateTime start, DateTime end)
        {
            var parameters = new DynamicParameters();
            parameters.Add("RoomTypeID", roomTypeId);
            parameters.Add("PackageID", packageId);
            parameters.Add("Start", start);
            parameters.Add("End", end);
            parameters.Add("Valid", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync("dbo.genVerifyRoomAvailabilityForPackageAllocation", parameters, commandType: CommandType.StoredProcedure);
            return parameters.Get<bool>("Valid");
        }

        public async Task<int> LowestAllocationAsync(int roomTypeId, int packageId, DateTime start, DateTime end)
        {
            var parameters = new DynamicParameters();
            parameters.Add("RoomTypeID", roomTypeId);
            parameters.Add("PackageID", packageId);
            parameters.Add("Start", start);
            parameters.Add("End", end);
            parameters.Add("Qty", dbType: DbType.Int32, direction: ParameterDirection.Output);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync("dbo.genSelLowQtyForPackageAllocation", parameters, commandType: CommandType.StoredProcedure);
            return parameters.Get<int>("Qty");
        }
    }
}
