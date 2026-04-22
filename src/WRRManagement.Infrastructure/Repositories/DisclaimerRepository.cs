using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class DisclaimerRepository : DapperRepository, IDisclaimerRepository
    {
        public DisclaimerRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<int> AddAsync(Disclaimer disclaimer)
        {
            var parameters = new
            {
                HotelID = disclaimer.HotelID,
                ReservationDisclaimer = disclaimer.ReservationDisclaimer,
                EmailDisclaimer = disclaimer.EmailDisclaimer
            };
            return await ExecuteScalarIntAsync("dbo.spDisclaimer_Insert", parameters);
        }

        public async Task<Disclaimer> GetDisclaimerForHotel(int hotelId)
        {
            var parameters = new {HotelID = hotelId};
            return await QueryFirstOrDefaultAsync<Disclaimer>("dbo.spDisclaimer_GetByHotelId", parameters);
        }

        public async Task<string?> GetEmailDisclaimerAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await ExecuteScalarStringAsync("dbo.spDisclaimer_GetEmail", parameters);
        }

        public async Task<string?> GetReservationDisclaimerAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await ExecuteScalarStringAsync("dbo.spDisclaimer_GetReservation", parameters);
        }

        public async Task UpdateEmailDisclaimerAsync(int hotelId, string text)
        {
            var parameters = new {HotelID = hotelId, EmailDisclaimer = text};
            await ExecuteAsync("dbo.spDisclaimer_UpdateEmail", parameters);
        }

        public async Task UpdateReservationDisclaimerAsync(int hotelId, string text)
        {
            var parameters = new {HotelID = hotelId, ReservationDisclaimer = text};
            await ExecuteAsync("dbo.spDisclaimer_UpdateReservation", parameters);
        }
    }
}
