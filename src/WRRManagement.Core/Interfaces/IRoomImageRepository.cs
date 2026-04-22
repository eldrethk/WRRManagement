using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IRoomImageRepository
    {
        Task<int> AddAsync(RoomImage img);
        Task<RoomImage> GetImageByIdAsync(int imageId);
        Task<IEnumerable<RoomImage>> GetAllImageForRoomAsync(int roomId);
        Task SetInvisibleAsync(int imageId);
        Task SetAsMainImageAsync(int imageId, int roomTypeID);
        Task<RoomImage> GetMainForRoomAsync(int roomTypeID);
    }
}
