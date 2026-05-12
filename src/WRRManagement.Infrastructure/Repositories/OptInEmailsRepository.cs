using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class OptInEmailsRepository : DapperRepository, IOptInEmailsRepository
    {
        public OptInEmailsRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<IEnumerable<OptInEmails>> GetAllAsync(int hotelId, DateTime start, DateTime end)
        {
            var parameters = new { HotelID = hotelId, Start = start, End = end };
            return await QueryAsync<OptInEmails>("dbo.genSelEmailList", parameters);
        }

        public async Task<int> AddAsync(string email, int hotelId, string firstName, string lastName, string state)
        {
            var parameters = new
            {
                EmailAddress = email,
                HotelID = hotelId,
                FirstName = firstName,
                LastName = lastName,
                State = state,
                OptInDate = DateTime.UtcNow
            };
            return await ExecuteScalarIntAsync("dbo.genInsOptInEmail", parameters);
        }
    }
}
