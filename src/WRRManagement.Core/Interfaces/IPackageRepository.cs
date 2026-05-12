using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IPackageRepository
    {
        Task<Package?> GetByIdAsync(int packageId);
        Task<IEnumerable<Package>> GetAllForHotelAsync(int hotelId);
        Task<IEnumerable<Package>> GetByRoomAsync(int roomTypeId, DateTime start, DateTime end);
        Task<IEnumerable<Package>> GetForSpecialPageAsync(int hotelId);
        Task<IEnumerable<Package>> GetWithRackRatesAsync(int hotelId);
        Task<IEnumerable<RoomType>> GetRoomTypesAsync(int packageId);
        Task<int> AddAsync(Package package);
        Task UpdateAsync(Package package);
        Task UpdateImageAsync(int packageId, string imagePath);
        Task SetInvisibleAsync(int packageId);
        Task SetRoomAssociationsAsync(int packageId, IEnumerable<int> roomTypeIds);
    }
}
