using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IRackRateRepository
    {
        Task<int> AddAsync(RackRate rackRate);
        Task<RackRate> GetByIdAsync(int rackRateId);
        Task<bool> CheckDatesAysnc(int roomId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<RackRate>> GetAllByRoomIdAsync(int roomId);
        Task<RackRate?> GetRateForDateAsync(int roomTypeId, DateTime date);
        Task InvisibleAsync(int rackRateId);
        Task UpdateAsync(RackRate rackRate);
        Task VisibleAsync(int rackRateId);
    }
}
