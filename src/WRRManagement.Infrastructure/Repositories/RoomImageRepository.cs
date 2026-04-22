using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class RoomImageRepository : DapperRepository, IRoomImageRepository
    {
        public RoomImageRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<int> AddAsync(RoomImage img)
        {
            var parameters = new
            {
                RoomTypeID = img.RoomTypeID,
                BlobUrl = img.BlobUrl,
                FileName = img.FileName,
                ContentType = img.ContentType,
                ContentLength = img.ContentLength,
                SortOrder = img.SortOrder,
                Description = img.Description

            };
            return await ExecuteScalarIntAsync("dbo.spRoomImage_Insert", parameters);
        }

        public async Task<IEnumerable<RoomImage>> GetAllImageForRoomAsync(int roomId)
        {
            var parameters = new {RoomTypeID =  roomId};
            return await QueryAsync<RoomImage>("dbo.spRoomImage_GetAllForRoom", parameters);
        }

        public async Task<RoomImage> GetImageByIdAsync(int imageId)
        {
            var parameters = new { ImageID =  imageId };
            return await QueryFirstOrDefaultAsync<RoomImage>("dbo.spRoomImage_GetById", parameters);
        }

        public async Task<RoomImage> GetMainForRoomAsync(int roomTypeID)
        {
            var parameters = new {RoomTypeID =  roomTypeID};
            return await QueryFirstOrDefaultAsync<RoomImage>("dbo.spRoomImage_GetMainForRoom", parameters);
        }

        public async Task SetAsMainImageAsync(int imageId, int roomTypeID)
        {
            var parameters = new {ImageID = imageId, RoomTypeID = roomTypeID};
            await ExecuteAsync("dbo.spRoomImage_SetAsMain", parameters);
        }

        public async Task SetInvisibleAsync(int imageId)
        {
            var parameters = new {ImageID = imageId};
            await ExecuteAsync("dbo.spRoomImage_SetInvisible", parameters);
        }
    }
}
