using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class TierLevelRepository : DapperRepository, ITierLevelRepository
    {
        public TierLevelRepository(IDbConnectionFactory dbConnectionFactory): base(dbConnectionFactory) { }

        public async Task<int> AddAsync(TierLevel tierLevel)
        {
            var parameters = new
            {
                HotelID = tierLevel.HotelID,
                Date = tierLevel.TierDate,
                Level = tierLevel.Tier
            };
            return await ExecuteScalarIntAsync("dbo.genInsTierLevel", parameters);
        }

        public async Task<Char> GetTierForDateAsync(int hotelId, DateTime date)
        {
            var parameters = new 
            {
                HotelID = hotelId,
                Date = date
            };
            return await ExecuteScalarCharAsync("dbo.genSelTierLevelByDate", parameters);
        }

        public async Task<IEnumerable<TierLevel>> GetAllForHotelAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<TierLevel>("dbo.genSelTierLevelByHotelID", parameters);
        }

        public async Task UpdateAsync(char tier, int tierLevelId)
        {
            var parameters = new {TierLevelID = tierLevelId, Tier =  tier};
            await ExecuteAsync("dbo.genUpdTierLevel", parameters);
        }
    }
}
