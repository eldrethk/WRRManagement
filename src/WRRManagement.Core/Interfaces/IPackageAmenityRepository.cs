using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IPackageAmenityRepository
    {
        Task<PackageAmenity?> GetAsync(int packageId, int extraAmenityId);
        Task<IEnumerable<PackageAmenity>> GetAllForPackageAsync(int packageId);
        Task<IEnumerable<ExtraAmenity>> GetMandatoryAmenitiesAsync(int packageId);
        Task<IEnumerable<Package>> GetPackagesForAmenityAsync(int extraAmenityId);
        Task<int> AddAsync(PackageAmenity packageAmenity);
        Task UpdateAsync(PackageAmenity packageAmenity);
        Task RemoveAsync(int extraAmenityId, int packageId);
    }
}
