using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface ICalendarMessageRepository
    {
        Task<CalendarMessage?> GetByIdAsync(int messageId);
        Task<IEnumerable<CalendarMessage>> GetAllForHotelAsync(int hotelId);
        Task<IEnumerable<CalendarMessage>> GetByDateAsync(DateTime date, int hotelId);
        Task<int> AddAsync(CalendarMessage message);
        Task UpdateAsync(CalendarMessage message);
        Task DeleteAsync(int messageId);
    }
}
