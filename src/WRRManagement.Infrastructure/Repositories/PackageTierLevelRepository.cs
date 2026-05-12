using Dapper;
using System.Data;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class PackageTierLevelRepository : DapperRepository, IPackageTierLevelRepository
    {
        public PackageTierLevelRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<PackageTierLevel?> GetByIdAsync(int tierLevelId)
        {
            var parameters = new { TierLevelID = tierLevelId };
            return await QueryFirstOrDefaultAsync<PackageTierLevel>("dbo.genSelPackageTierLevelByID", parameters);
        }

        public async Task<IEnumerable<PackageTierLevel>> GetForPackageAsync(int packageId)
        {
            var parameters = new { PackageID = packageId };
            return await QueryAsync<PackageTierLevel>("dbo.genSelPackageTierLevelByPackageID", parameters);
        }

        public async Task<IEnumerable<Package>> GetPackagesWithTierAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<Package>("dbo.genSelPackagesWithTierLevel", parameters);
        }

        public async Task<char> GetTierForDateAsync(int packageId, DateTime date)
        {
            var parameters = new DynamicParameters();
            parameters.Add("PackageID", packageId);
            parameters.Add("Date", date);
            parameters.Add("Tier", dbType: DbType.StringFixedLength, size: 1, direction: ParameterDirection.Output);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync("dbo.genSelPackageTierLevelByDate", parameters, commandType: CommandType.StoredProcedure);
            return parameters.Get<char>("Tier");
        }

        public async Task<int> AddAsync(int packageId, DateTime date, char tier)
        {
            var parameters = new { PackageID = packageId, TierDate = date, Tier = tier };
            return await ExecuteScalarIntAsync("dbo.genInsPackageTierLevel", parameters);
        }

        public async Task AddDateRangeAsync(int packageId, DateTime start, DateTime end, char tier)
        {
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                await AddAsync(packageId, date, tier);
            }
        }

        public async Task UpdateAsync(char tier, int tierLevelId)
        {
            var parameters = new { TierLevelID = tierLevelId, Tier = tier };
            await ExecuteAsync("dbo.genUpdPackageTierLevel", parameters);
        }
    }
}
