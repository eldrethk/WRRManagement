using Dapper;
using System.Data;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class DashboardRepository : DapperRepository, IDashboardRepository
    {
        public DashboardRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<ReservationQue>> GetReservationQueAsync(int hotelId)
        {
            // rptSelReservationQue only returns QueID — query the table directly for all columns
            const string sql = """
                SELECT QueID, ReservationID, BookedDate, CustomerName,
                       ReservationType, Viewed, HotelID, UserName
                FROM   ReservationQue
                WHERE  HotelID = @HotelID
                  AND  BookedDate BETWEEN DATEADD(Day, -14, GETDATE()) AND GETDATE()
                ORDER BY Viewed, BookedDate DESC
                """;

            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<ReservationQue>(sql, new { HotelID = hotelId });
        }

        public async Task MarkViewedAsync(int reservationId, string userName, int hotelId)
        {
            var parameters = new { ReservationID = reservationId, UserName = userName, HotelID = hotelId };
            await ExecuteAsync("dbo.rptUpdReservationQue", parameters);
        }

        public async Task<ReservationStats> GetRackRateStatsAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryFirstOrDefaultAsync<ReservationStats>(
                "dbo.rptSelReservationBookedPast30Days", parameters)
                ?? new ReservationStats();
        }

        public async Task<ReservationStats> GetPackageStatsAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryFirstOrDefaultAsync<ReservationStats>(
                "dbo.rptSelPackageBookedPast30Days", parameters)
                ?? new ReservationStats();
        }

        public async Task<AmenityStats> GetAmenityStatsAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryFirstOrDefaultAsync<AmenityStats>(
                "dbo.rptSelAmenitiesBookedPast30Days", parameters)
                ?? new AmenityStats();
        }

        // These procs use RETURN not SELECT — must use DynamicParameters with ReturnValue
        public async Task<int> GetTodaysArrivalsAsync(int hotelId)
            => await ExecuteReturnProcAsync("dbo.rptTodaysArrival", hotelId);

        public async Task<int> GetTodaysDeparturesAsync(int hotelId)
            => await ExecuteReturnProcAsync("dbo.rptTodaysDeparture", hotelId);

        public async Task<int> GetOccupiedRoomsAsync(int hotelId)
            => await ExecuteReturnProcAsync("dbo.rptTodaysRoomOccupied", hotelId);

        private async Task<int> ExecuteReturnProcAsync(string procName, int hotelId)
        {
            var p = new DynamicParameters(new { HotelID = hotelId });
            p.Add("ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(procName, p, commandType: CommandType.StoredProcedure);
            return p.Get<int>("ReturnValue");
        }
    }
}
