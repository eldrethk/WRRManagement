using Dapper;
using System.Data;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class PackageRateRepository : DapperRepository, IPackageRateRepository
    {
        public PackageRateRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<PackageRate?> GetByIdAsync(int rateId)
        {
            var parameters = new { RateID = rateId };
            return await QueryFirstOrDefaultAsync<PackageRate>("dbo.genSelPackageRateByID", parameters);
        }

        public async Task<IEnumerable<PackageRate>> GetRatesAsync(int roomTypeId, int packageId)
        {
            var parameters = new { RoomTypeID = roomTypeId, PackageID = packageId };
            return await QueryAsync<PackageRate>("dbo.genSelPackageRates", parameters);
        }

        public async Task<PackageRate?> GetRateForDateAsync(int roomTypeId, DateTime date, int packageId)
        {
            var parameters = new { RoomTypeID = roomTypeId, Date = date, PackageID = packageId };
            return await QueryFirstOrDefaultAsync<PackageRate>("dbo.genSelPackageRateByDate", parameters);
        }

        public async Task<bool> CheckDatesAsync(int roomTypeId, DateTime start, DateTime end, int packageId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("RoomTypeID", roomTypeId);
            parameters.Add("Start", start);
            parameters.Add("End", end);
            parameters.Add("PackageID", packageId);
            parameters.Add("Valid", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync("dbo.genVerifyPackageRates", parameters, commandType: CommandType.StoredProcedure);
            return parameters.Get<bool>("Valid");
        }

        public async Task<int> AddAsync(PackageRate rate)
        {
            var parameters = new
            {
                rate.PackageID,
                rate.RoomTypeID,
                rate.StartDate,
                rate.EndDate,
                rate.Price,
                rate.Visible
            };
            return await ExecuteScalarIntAsync("dbo.genInsPackageRate", parameters);
        }

        public async Task UpdateAsync(PackageRate rate)
        {
            var parameters = new
            {
                rate.RateID,
                rate.StartDate,
                rate.EndDate,
                rate.Price,
                rate.Visible
            };
            await ExecuteAsync("dbo.genUpdPackageRate", parameters);
        }

        public async Task SetInvisibleAsync(int rateId)
        {
            var parameters = new { RateID = rateId };
            await ExecuteAsync("dbo.genInvisiblePackageRate", parameters);
        }

        public async Task SetVisibleAsync(int rateId)
        {
            var parameters = new { RateID = rateId };
            await ExecuteAsync("dbo.genVisiblePackageRate", parameters);
        }
    }
}
