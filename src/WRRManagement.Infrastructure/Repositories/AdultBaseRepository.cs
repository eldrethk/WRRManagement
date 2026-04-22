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
    public class AdultBaseRepository : DapperRepository, IAdultBaseRepository
    {
        public AdultBaseRepository(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        public async Task<int> AddAsync(AdultBase adult)
        {
            var parameters = new {
                RoomID = adult.RoomTypeID,
                AdultBase = adult.AdultBaseCount,
                ChildBase = adult.ChildBaseCount,
                AdultMax = adult.MaxAdult,
                ChildMax = adult.MaxAdult,
                Total = adult.MaxRoomTotal
            };
            return await ExecuteScalarIntAsync("dbo.genInsAdultBase", parameters);
        }

        public async Task<AdultBase> GetByRoomIDAsync(int roomID)
        {
            var parameters = new { RoomID = roomID };
            return await QueryFirstOrDefaultAsync<AdultBase>("dbo.genSelAdultBaseByID", parameters);
        }
    }
}
