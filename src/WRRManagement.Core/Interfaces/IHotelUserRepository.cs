using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IHotelUserRepository
    {
        Task<IEnumerable<Hotel>> GetHotelsForUser(string userId);

        Task<int> AddUserToHotel(string userId, int hotelId);

        Task<IEnumerable<HotelUser>> GetUserForHotel(int hotelId);
    }
}
