using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IPackageAllocationRepository
    {
        Task<PackageAllocation?> GetByIdAsync(int allocationId);
        Task<IEnumerable<PackageAllocation>> GetForPackageRoomAsync(int packageId, int roomTypeId);
        Task<IEnumerable<Package>> GetPackagesWithAllocationsAsync(int hotelId);
        Task<int> AddAsync(int roomTypeId, int packageId, DateTime date, int quantity);
        Task AddDateRangeAsync(int roomTypeId, int packageId, DateTime start, DateTime end, int quantity);
        Task UpdateAsync(int quantity, int allocationId);
        Task RollbackAsync(int roomTypeId, int packageId, DateTime date);
        Task<bool> IsValidAsync(int roomTypeId, int packageId, DateTime start, DateTime end);
        Task<int> LowestAllocationAsync(int roomTypeId, int packageId, DateTime start, DateTime end);
    }
}
