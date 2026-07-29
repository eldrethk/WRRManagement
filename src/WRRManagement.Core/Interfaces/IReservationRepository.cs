using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IReservationRepository
    {
        Task<int> CreateAsync(Reservation reservation);
        Task AddDailyRatesAsync(int reservationId, IEnumerable<(DateTime date, decimal rate)> dailyRates);
        Task<int?> GetIdByIdempotencyKeyAsync(Guid idempotencyKey);
    }
}
