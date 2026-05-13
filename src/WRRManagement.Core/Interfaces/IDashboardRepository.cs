using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IDashboardRepository
    {
        Task<IEnumerable<ReservationQue>> GetReservationQueAsync(int hotelId);
        Task MarkViewedAsync(int reservationId, string userName, int hotelId);
        Task<ReservationStats> GetRackRateStatsAsync(int hotelId);
        Task<ReservationStats> GetPackageStatsAsync(int hotelId);
        Task<AmenityStats> GetAmenityStatsAsync(int hotelId);
        Task<int> GetTodaysArrivalsAsync(int hotelId);
        Task<int> GetTodaysDeparturesAsync(int hotelId);
        Task<int> GetOccupiedRoomsAsync(int hotelId);
    }
}
