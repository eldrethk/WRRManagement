using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IPackageRateRepository
    {
        Task<PackageRate?> GetByIdAsync(int rateId);
        Task<IEnumerable<PackageRate>> GetRatesAsync(int roomTypeId, int packageId);
        Task<PackageRate?> GetRateForDateAsync(int roomTypeId, DateTime date, int packageId);
        Task<bool> CheckDatesAsync(int roomTypeId, DateTime start, DateTime end, int packageId);
        Task<int> AddAsync(PackageRate rate);
        Task UpdateAsync(PackageRate rate);
        Task SetInvisibleAsync(int rateId);
        Task SetVisibleAsync(int rateId);
    }
}
