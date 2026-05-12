using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class PackageAmenityRepository : DapperRepository, IPackageAmenityRepository
    {
        public PackageAmenityRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<PackageAmenity?> GetAsync(int packageId, int extraAmenityId)
        {
            var parameters = new { PackageID = packageId, ExtraAmenityID = extraAmenityId };
            return await QueryFirstOrDefaultAsync<PackageAmenity>("dbo.genSelPackageAmenityByID", parameters);
        }

        public async Task<IEnumerable<PackageAmenity>> GetAllForPackageAsync(int packageId)
        {
            var parameters = new { PackageID = packageId };
            return await QueryAsync<PackageAmenity>("dbo.genSelPackageAmenities", parameters);
        }

        public async Task<IEnumerable<ExtraAmenity>> GetMandatoryAmenitiesAsync(int packageId)
        {
            var parameters = new { PackageID = packageId };
            return await QueryAsync<ExtraAmenity>("dbo.genSelMandatoryAmenitiesForPackage", parameters);
        }

        public async Task<IEnumerable<Package>> GetPackagesForAmenityAsync(int extraAmenityId)
        {
            var parameters = new { ExtraAmenityID = extraAmenityId };
            return await QueryAsync<Package>("dbo.genSelPackageAssociatedWithAmenity", parameters);
        }

        public async Task<int> AddAsync(PackageAmenity packageAmenity)
        {
            var parameters = new
            {
                packageAmenity.PackageID,
                packageAmenity.ExtraAmenityID,
                packageAmenity.ViewRate,
                packageAmenity.Mandatory,
                packageAmenity.MandatoryQuantity,
                packageAmenity.AdditionalPurchases
            };
            return await ExecuteScalarIntAsync("dbo.genInsPackageAmenity", parameters);
        }

        public async Task UpdateAsync(PackageAmenity packageAmenity)
        {
            var parameters = new
            {
                packageAmenity.PackageAmenityID,
                packageAmenity.ViewRate,
                packageAmenity.Mandatory,
                packageAmenity.MandatoryQuantity,
                packageAmenity.AdditionalPurchases
            };
            await ExecuteAsync("dbo.genUpdPackageAmenity", parameters);
        }

        public async Task RemoveAsync(int extraAmenityId, int packageId)
        {
            var parameters = new { ExtraAmenityID = extraAmenityId, PackageID = packageId };
            await ExecuteAsync("dbo.genDelPackageAmenity", parameters);
        }
    }
}
