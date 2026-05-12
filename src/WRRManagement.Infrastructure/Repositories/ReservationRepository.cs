using Dapper;
using System.Data;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class ReservationRepository : DapperRepository, IReservationRepository
    {
        public ReservationRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<int> CreateAsync(Reservation reservation)
        {
            return await ExecuteWithTransactionAsync<int>(async (connection, transaction) =>
            {
                // Decrement room allocation for each night of the stay
                for (var date = reservation.ArrivalDate; date < reservation.DepartureDate; date = date.AddDays(1))
                {
                    await connection.ExecuteAsync("dbo.genReserveRoom",
                        new { RoomID = reservation.RoomTypeID, Date = date },
                        transaction,
                        commandType: CommandType.StoredProcedure);
                }

                // Insert the reservation record and return the new ID
                var parameters = new
                {
                    reservation.HotelID,
                    reservation.RoomTypeID,
                    reservation.PaymentTypeID,
                    reservation.ArrivalDate,
                    reservation.DepartureDate,
                    reservation.TotalNights,
                    reservation.Adults,
                    reservation.Children,
                    reservation.AvgDailyRate,
                    reservation.SubTotal,
                    reservation.TierLevel,
                    reservation.ExtraAdultCharge,
                    reservation.ExtraChildCharge,
                    reservation.WeekendFees,
                    reservation.ResortFees,
                    reservation.TotalFees,
                    reservation.Taxes,
                    reservation.TotalCharge,
                    reservation.Deposit,
                    reservation.ExtraFees,
                    reservation.Comments,
                    reservation.CardHolderName,
                    reservation.CardExpirationDate,
                    reservation.CardNumber,
                    reservation.CardSecureCode,
                    reservation.CusFirstName,
                    reservation.CusLastName,
                    reservation.CusAddress1,
                    reservation.CusAddress2,
                    reservation.CusCity,
                    reservation.CusState,
                    reservation.CusZip,
                    reservation.CusDayPhone,
                    reservation.CusEveningPhone,
                    reservation.CusEmail,
                    reservation.BookedAmenity,
                    reservation.UserInitials,
                    reservation.ReservationCreated,
                    reservation.SessionID,
                    reservation.CustomerId
                };

                return await connection.ExecuteScalarAsync<int>("dbo.genInsReservation",
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            });
        }

        public async Task AddDailyRatesAsync(int reservationId, IEnumerable<(DateTime date, decimal rate)> dailyRates)
        {
            foreach (var (date, rate) in dailyRates)
            {
                var parameters = new { ReservationID = reservationId, Date = date, Rate = rate };
                await ExecuteAsync("dbo.genInsDailyRate", parameters);
            }
        }
    }
}
