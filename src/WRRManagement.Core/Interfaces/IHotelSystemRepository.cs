using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IHotelSystemRepository
    {
        Task<int> GetPriorDayBookingAsync(int hotelId);
        Task<HotelSystem> GetSystemAsync(int hotelId);

        Task UpdateAsync(HotelSystem hotelSystem);
    }
}
