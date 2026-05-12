using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IRoomAllocation
    {
        Task<int> AddAsync(RoomAllocation allocation);
        Task<bool> AllocationIsValidAsync(int roomId, DateTime start,  DateTime end);
        Task<IEnumerable<RoomAllocation>> GetAllForRoomAsync(int roomId);
        Task<RoomAllocation> GetByIdAsync(int allocationId);
        Task<int> GetQuantityForDayAsync(int roomId, DateTime date);
        Task UpdateQuantityAsync(int qty, int allocationId);
        Task<int> LowestAllocationAsync(int roomTypeId, DateTime start, DateTime end);
    }
}
