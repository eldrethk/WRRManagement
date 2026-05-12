using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class CalendarMessageRepository : DapperRepository, ICalendarMessageRepository
    {
        public CalendarMessageRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<CalendarMessage?> GetByIdAsync(int messageId)
        {
            var parameters = new { MessageID = messageId };
            return await QueryFirstOrDefaultAsync<CalendarMessage>("dbo.genSelCustomMessageByID", parameters);
        }

        public async Task<IEnumerable<CalendarMessage>> GetAllForHotelAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<CalendarMessage>("dbo.genSelCustomMessageByHotelID", parameters);
        }

        public async Task<IEnumerable<CalendarMessage>> GetByDateAsync(DateTime date, int hotelId)
        {
            var parameters = new { Date = date, HotelID = hotelId };
            return await QueryAsync<CalendarMessage>("dbo.genSelCustomMessageByDate", parameters);
        }

        public async Task<int> AddAsync(CalendarMessage message)
        {
            var parameters = new
            {
                message.HotelID,
                message.Message,
                message.DisplayFrom,
                message.DisplayTo
            };
            return await ExecuteScalarIntAsync("dbo.genInsCustomMessage", parameters);
        }

        public async Task UpdateAsync(CalendarMessage message)
        {
            var parameters = new
            {
                message.MessageID,
                message.Message,
                message.DisplayFrom,
                message.DisplayTo
            };
            await ExecuteAsync("dbo.genUpdCustomMessage", parameters);
        }

        public async Task DeleteAsync(int messageId)
        {
            var parameters = new { MessageID = messageId };
            await ExecuteAsync("dbo.genDelCustomMessage", parameters);
        }
    }
}
