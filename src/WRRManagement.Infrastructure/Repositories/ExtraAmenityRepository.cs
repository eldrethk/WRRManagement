using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRRManagement.Infrastructure.Data;

namespace WRRManagement.Infrastructure.Repositories
{
    public class ExtraAmenityRepository : DapperRepository, IExtraAmenityRepository
    {
        public ExtraAmenityRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory) { }

        public async Task<ExtraAmenity?> GetByIdAsync(int amenityId)
        {
            var parameters = new { AmenityID = amenityId };
            return await QueryFirstOrDefaultAsync<ExtraAmenity>("dbo.genSelExtraAmenityByID", parameters);
        }

        public async Task<IEnumerable<ExtraAmenity>> GetAllForHotelAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<ExtraAmenity>("dbo.genSelExtraAmenity", parameters);
        }

        public async Task<IEnumerable<ExtraAmenity>> GetRackRateAmenitiesAsync(int hotelId)
        {
            var parameters = new { HotelID = hotelId };
            return await QueryAsync<ExtraAmenity>("dbo.genSelRackRateAmenities", parameters);
        }

        public async Task<IEnumerable<ExtraAmenity>> GetPackageAmenitiesAsync(int packageId)
        {
            var parameters = new { PackageID = packageId };
            return await QueryAsync<ExtraAmenity>("dbo.genSelExtraAmenitiesByPackageID", parameters);
        }

        public async Task<int> AddAsync(ExtraAmenity amenity)
        {
            var parameters = new
            {
                amenity.HotelID,
                amenity.Name,
                amenity.ShortDescription,
                amenity.Description,
                amenity.AmenityRate,
                amenity.Tax,
                amenity.ViewRate,
                amenity.Mandatory,
                amenity.Visible,
                amenity.PerDayPerPerson,
                amenity.PerDay,
                amenity.PerNightStay,
                amenity.OneTimeFee,
                amenity.OneTimeFeePerson,
                amenity.Discount,
                amenity.DiscountRegularRate,
                amenity.PictureUrl,
                amenity.ViewOnRackRate,
                amenity.MandatoryQty,
                amenity.AdditionalPurchases
            };
            return await ExecuteScalarIntAsync("dbo.genInsExtraAmenity", parameters);
        }

        public async Task UpdateAsync(ExtraAmenity amenity)
        {
            var parameters = new
            {
                amenity.AmenityID,
                amenity.Name,
                amenity.ShortDescription,
                amenity.Description,
                amenity.AmenityRate,
                amenity.Tax,
                amenity.ViewRate,
                amenity.Mandatory,
                amenity.Visible,
                amenity.PerDayPerPerson,
                amenity.PerDay,
                amenity.PerNightStay,
                amenity.OneTimeFee,
                amenity.OneTimeFeePerson,
                amenity.Discount,
                amenity.DiscountRegularRate,
                amenity.PictureUrl,
                amenity.ViewOnRackRate,
                amenity.MandatoryQty,
                amenity.AdditionalPurchases
            };
            await ExecuteAsync("dbo.genUpdExtraAmenity", parameters);
        }

        public async Task RemoveAsync(int amenityId)
        {
            var parameters = new { AmenityID = amenityId };
            await ExecuteAsync("dbo.genInvisbleExtraAmenity", parameters);
        }
    }
}
