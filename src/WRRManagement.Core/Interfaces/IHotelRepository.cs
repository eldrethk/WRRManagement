using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IHotelRepository
    {
        Task<Hotel?> GetByIdAsync(int hotelId);
        Task UpdateAsync(Hotel hotel);
    }
}
