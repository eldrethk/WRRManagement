using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IDisclaimerRepository
    {
        Task<string> GetReservationDisclaimerAsync(int hotelId);
        Task<string> GetEmailDisclaimerAsync(int hotelId);
        Task UpdateReservationDisclaimerAsync(int hotelId, string text);
        Task UpdateEmailDisclaimerAsync(int hotelId, string text);
        Task<int> AddAsync(Disclaimer disclaimer);
        Task<Disclaimer> GetDisclaimerForHotel(int hotelId);

    }
}
