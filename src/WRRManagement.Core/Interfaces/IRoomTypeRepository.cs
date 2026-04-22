using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IRoomTypeRepository
    {
        Task<RoomType> GetByIdAsync(int roomTypeId);
        Task<IEnumerable<RoomType>> GetAllForHotelAsync(int hotelId);
        Task InvisibleAsync(int roomTypeId);
        Task<int> AddAsync(RoomType roomType);
        Task UpdateAsync(RoomType roomType);
    }
}
