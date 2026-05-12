using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IReservationAmenityRepository
    {
        Task<IEnumerable<ReservationAmenity>> GetBookedAmenitiesAsync(int reservationId);
        Task<int> AddAsync(ReservationAmenity reservationAmenity);
    }
}
