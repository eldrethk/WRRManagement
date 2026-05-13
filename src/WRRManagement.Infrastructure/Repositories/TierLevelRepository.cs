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

        public async Task<int> AddAsync(int hotelId, DateTime date, char tier)
        {
            var parameters = new
            {
                HotelID = hotelId,
                Date = date,
                Level = tier
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

        public async Task AddDateRangeAsync(int hotelID,  DateTime start, DateTime end, char tier)
        {
            if (start > DateTime.MinValue && end > DateTime.MinValue && hotelID > 0)
            {
                DateTime temp = start;
                while (temp <= end)
                {
                    int id = await AddAsync(hotelID, temp, tier);
                    temp = temp.AddDays(1);
                }
            }
        }
    }
}
