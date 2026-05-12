using Microsoft.Extensions.Logging;
using WRRManagement.Application.Amenities.Dtos;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Amenities
{
    public class AmenityQueryService : IAmenityQueryService
    {
        private readonly IExtraAmenityRepository _amenityRepo;
        private readonly ILogger<AmenityQueryService> _logger;

        public AmenityQueryService(IExtraAmenityRepository amenityRepo, ILogger<AmenityQueryService> logger)
        {
            _amenityRepo = amenityRepo;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ExtraAmenityDto>> GetAmenitiesForHotelAsync(int hotelId, CancellationToken ct = default)
        {
            var amenities = await _amenityRepo.GetAllForHotelAsync(hotelId);
            return amenities
                .Where(a => a.Visible)
                .Select(a => new ExtraAmenityDto
                {
                    AmenityId = a.AmenityID,
                    Name = a.Name,
                    ShortDescription = a.ShortDescription,
                    Description = a.Description,
                    AmenityRate = a.AmenityRate,
                    Tax = a.Tax,
                    ViewRate = a.ViewRate,
                    Mandatory = a.Mandatory,
                    PerDayPerPerson = a.PerDayPerPerson,
                    PerDay = a.PerDay,
                    PerNightStay = a.PerNightStay,
                    OneTimeFee = a.OneTimeFee,
                    OneTimeFeePerson = a.OneTimeFeePerson,
                    Discount = a.Discount,
                    DiscountRegularRate = a.DiscountRegularRate,
                    PictureUrl = a.PictureUrl,
                    ViewOnRackRate = a.ViewOnRackRate,
                    MandatoryQty = a.MandatoryQty,
                    AdditionalPurchases = a.AdditionalPurchases
                })
                .ToList();
        }

        public async Task<bool> HotelHasAmenitiesAsync(int hotelId, CancellationToken ct = default)
        {
            var amenities = await GetAmenitiesForHotelAsync(hotelId, ct);
            return amenities.Count > 0;
        }
    }
}
