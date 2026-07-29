using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class ReservationAmenityRepository : DapperRepository, IReservationAmenityRepository
    {
        public ReservationAmenityRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<ReservationAmenity>> GetBookedAmenitiesAsync(int reservationId)
        {
            var parameters = new { ReservationID = reservationId };
            return await QueryAsync<ReservationAmenity>("dbo.genSelReservationAmenity", parameters);
        }

        public async Task<int> AddAsync(ReservationAmenity reservationAmenity)
        {
            var parameters = new
            {
                reservationAmenity.ReservationID,
                reservationAmenity.AmenityID,
                reservationAmenity.ChargeAmount,
                reservationAmenity.TaxIncluded,
                reservationAmenity.Mandatory,
                reservationAmenity.TaxRate,
                reservationAmenity.NumPeople,
                NumNights = reservationAmenity.NumNights,
                reservationAmenity.TotalCharge
            };
            return await ExecuteScalarIntAsync("dbo.genInsReservationAmenity", parameters);
        }
    }
}
