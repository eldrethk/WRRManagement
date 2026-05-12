using WRRManagement.Application.Rooms.Dtos;

namespace WRRManagement.Application.Rooms
{
    public interface IRoomQueryService
    {
        Task<IReadOnlyList<ViewRoomDto>> GetBookableRoomsAsync(int hotelId, CancellationToken ct = default);
        Task<IReadOnlyList<AvailableRackRoomDto>> SearchAvailabilityAsync(int hotelId, DateTime checkIn, DateTime checkOut, int adults, int children, CancellationToken ct = default);
        Task<bool> IsRoomAvailableAsync(int roomTypeId, DateTime checkIn, DateTime checkOut, CancellationToken ct = default);
    }
}
