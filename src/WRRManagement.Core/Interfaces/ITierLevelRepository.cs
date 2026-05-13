using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface ITierLevelRepository
    {
        Task<char> GetTierForDateAsync(int hotelId, DateTime date);
        Task<IEnumerable<TierLevel>> GetAllForHotelAsync(int hotelId);
        Task<int> AddAsync(TierLevel tierLevel);
        Task UpdateAsync(char tier, int tierLevelId); Task AddDateRangeAsync(int hotelID, DateTime start, DateTime end, char tier);
    }
}
