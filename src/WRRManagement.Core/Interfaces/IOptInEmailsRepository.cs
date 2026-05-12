using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IOptInEmailsRepository
    {
        Task<IEnumerable<OptInEmails>> GetAllAsync(int hotelId, DateTime start, DateTime end);
        Task<int> AddAsync(string email, int hotelId, string firstName, string lastName, string state);
    }
}
