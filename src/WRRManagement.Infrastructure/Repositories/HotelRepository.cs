using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class HotelRepository : DapperRepository, IHotelRepository
    {        

        public HotelRepository(IDbConnectionFactory connectionsFactory) : base(connectionsFactory) 
        {
            
        }

        public async Task<Hotel?> GetByIdAsync(int hotelId)
        { 

            var parameters = new { HotelID = hotelId };
            var hotel = await QueryFirstOrDefaultAsync<Hotel>("dbo.genSelHotel",parameters);

            return hotel;
        }

        public async Task UpdateAsync(Hotel hotel)
        {
           
            var parameters = new
            {
                hotel.HotelID,
                hotel.Name,
                hotel.Email,
                hotel.AdminEmail,
                hotel.Address1,
                hotel.Address2,
                hotel.City,
                hotel.State,
                hotel.ZipCode,
                hotel.LocalPhone,
                hotel.TollFreePhone,
                hotel.Website,
                hotel.Description,
                hotel.CheckIn,
                hotel.CheckOut
            };

            await ExecuteAsync("dbo.genUpdHotel", parameters);
                
        }
    }
}
