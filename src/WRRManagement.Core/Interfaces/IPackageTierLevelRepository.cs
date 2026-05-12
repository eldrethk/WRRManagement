using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IPackageTierLevelRepository
    {
        Task<PackageTierLevel?> GetByIdAsync(int tierLevelId);
        Task<IEnumerable<PackageTierLevel>> GetForPackageAsync(int packageId);
        Task<IEnumerable<Package>> GetPackagesWithTierAsync(int hotelId);
        Task<char> GetTierForDateAsync(int packageId, DateTime date);
        Task<int> AddAsync(int packageId, DateTime date, char tier);
        Task AddDateRangeAsync(int packageId, DateTime start, DateTime end, char tier);
        Task UpdateAsync(char tier, int tierLevelId);
    }
}
