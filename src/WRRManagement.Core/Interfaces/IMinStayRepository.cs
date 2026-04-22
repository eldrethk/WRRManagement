using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IMinStayRepository
    {
        Task<int> AddAsync(MinStay minStay);
        Task<MinStay> GetByIdAsync(int minStayID);
        Task<IEnumerable<MinStay>> GetAllForRoomAsync(int roomId);
        Task<int> GetQuantityForDateAsync(int roomId, DateTime date);
        Task UpdateAsync(int Quantity, int minStayId);
    }
}
