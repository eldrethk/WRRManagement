using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IRoomFeaturesRepository
    {
        Task<int> AddAsync(RoomFeatures features);
        Task DeleteAsync(int id);
        Task<IEnumerable<RoomFeatures>> GetRoomFeaturesAsync(int roomId);
        Task UpdateAsync(RoomFeatures features);
    }
}
