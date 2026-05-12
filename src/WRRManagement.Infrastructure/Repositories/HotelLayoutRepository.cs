using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class HotelLayoutRepository : DapperRepository, IHotelLayoutRepository
    {
        public HotelLayoutRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<HotelLayout?> GetByHotelIdAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryFirstOrDefaultAsync<HotelLayout>("dbo.genSelHotelLayout", parameters);
        }

        public async Task UpdateCssAsync(int hotelId, string css)
        {
            var parameters = new { HotelID = hotelId, HotelCSS = css };
            await ExecuteAsync("dbo.genInsHotelCSS", parameters);
        }

        public async Task UpdateHeaderAsync(int hotelId, string headerFileName)
        {
            var parameters = new { HotelID = hotelId, HeaderFileName = headerFileName };
            await ExecuteAsync("dbo.genInsHotelHeader", parameters);
        }

        public async Task UpdateFooterAsync(int hotelId, string footerFileName)
        {
            var parameters = new { HotelID = hotelId, FooterFileName = footerFileName };
            await ExecuteAsync("dbo.genInsHotelFooter", parameters);
        }

        public async Task UpdateEmailLayoutAsync(int hotelId, string logo, string emailHeader)
        {
            var parameters = new { HotelID = hotelId, EmailHotelLogo = logo, EmailHeaderImage = emailHeader };
            await ExecuteAsync("dbo.genInsHotelEmail", parameters);
        }
    }
}
