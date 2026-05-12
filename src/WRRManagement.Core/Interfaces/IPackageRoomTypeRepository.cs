using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IPackageRoomTypeRepository
    {
        Task<IEnumerable<RoomType>> GetRoomTypesForPackageAsync(int packageId);
        Task SetRoomAssociationsAsync(int packageId, IEnumerable<int> roomTypeIds);
    }
}
