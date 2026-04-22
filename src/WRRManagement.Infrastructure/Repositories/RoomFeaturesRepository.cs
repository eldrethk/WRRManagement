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
    public class RoomFeaturesRepository : DapperRepository, IRoomFeaturesRepository
    {
        public RoomFeaturesRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<int> AddAsync(RoomFeatures features)
        {
            var parameters = new 
            {
                RoomID = features.RoomTypeID,
                Icon = features.Icon,
                Feature = features.Features
            };
            return await ExecuteScalarIntAsync("dbo.genInsRoomFeature", parameters);
        }

        public async Task DeleteAsync(int id)
        {
            var parameters = new {ID =  id};
            await ExecuteAsync("dbo.genDelRoomFeature", parameters);
        }

        public async Task<IEnumerable<RoomFeatures>> GetRoomFeaturesAsync(int roomId)
        {
            var parameters = new {RoomTypeID = roomId};
            return await QueryAsync<RoomFeatures>("dbo.genSelRoomFeature", parameters);
        }

        public async Task UpdateAsync(RoomFeatures features)
        {
            var parameters = new 
            {
                ID = features.ID,
                Icon = features.Icon,
                Feature = features.Features
            };

            await ExecuteAsync("dbo.genUpdRoomFeature", parameters);

        }
    }
}
