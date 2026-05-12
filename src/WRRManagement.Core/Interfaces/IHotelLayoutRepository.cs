using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IHotelLayoutRepository
    {
        Task<HotelLayout?> GetByHotelIdAsync(int hotelId);
        Task UpdateCssAsync(int hotelId, string css);
        Task UpdateHeaderAsync(int hotelId, string headerFileName);
        Task UpdateFooterAsync(int hotelId, string footerFileName);
        Task UpdateEmailLayoutAsync(int hotelId, string logo, string emailHeader);
    }
}
