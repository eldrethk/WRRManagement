using Dapper;
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
    public class HotelSystemRepository : DapperRepository, IHotelSystemRepository
    {
        public HotelSystemRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) 
        {
            
        }
        public async Task<int> GetPriorDayBookingAsync(int hotelId)
        {
            int days = 0;
           ;

            var parameters = new {HotelID = hotelId};
            days = await QueryFirstOrDefaultAsync<int>("dbo.genSelPriorDays", parameters);
            
            return days;
        }

        public async Task<HotelSystem> GetSystemAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId};
            var system = await QueryFirstOrDefaultAsync<HotelSystem>("dbo.genSelHotelSystem", parameters);
            
            return system;
        }

        public async Task UpdateAsync(HotelSystem hotelSystem)
        {
           
            var parameters = new
            {
                hotelSystem.HotelID,
                hotelSystem.ExtraAdultFee,
                hotelSystem.ExtraChildFee,
                hotelSystem.ExtraBaseFee,
                hotelSystem.WeekendFee,
                hotelSystem.ResortFee,
                hotelSystem.TaxRate,
                hotelSystem.AddTaxToDeposit,
                hotelSystem.RoomRateDisplayAs,
                hotelSystem.PackageRateDisplayAs,
                hotelSystem.LowAllocationLimit,
                hotelSystem.PriorBook,
                hotelSystem.RoomDepositCalAs,
                hotelSystem.PackageDepositCalAs,
                hotelSystem.DepositRoomPercentage,
                hotelSystem.DepositPackagePercentage,
                hotelSystem.AddTaxToWeekendFee,
                hotelSystem.AddTaxToExtraPerson,
                hotelSystem.HotelResortFeeCalAs,
                hotelSystem.AddTaxToResortFee,
                hotelSystem.RoomRateBreakdownAs,
                hotelSystem.PackageRateBreakdownAs

            };

            await ExecuteAsync("dbo.genUpdHotelSystem", parameters); 
        }
    }
}
