using Microsoft.Extensions.Logging;
using WRRManagement.Application.Hotels.Dtos;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Hotels
{
    public class HotelQueryService : IHotelQueryService
    {
        private readonly IHotelRepository _hotelRepo;
        private readonly IDisclaimerRepository _disclaimerRepo;
        private readonly ILogger<HotelQueryService> _logger;

        public HotelQueryService(
            IHotelRepository hotelRepo,
            IDisclaimerRepository disclaimerRepo,
            ILogger<HotelQueryService> logger)
        {
            _hotelRepo = hotelRepo;
            _disclaimerRepo = disclaimerRepo;
            _logger = logger;
        }

        public async Task<HotelInfoDto?> GetHotelAsync(int hotelId, CancellationToken ct = default)
        {
            var hotel = await _hotelRepo.GetByIdAsync(hotelId);
            if (hotel == null) return null;

            return new HotelInfoDto
            {
                HotelId = hotel.HotelID,
                Name = hotel.Name,
                Email = hotel.Email,
                AdminEmail = hotel.AdminEmail,
                Address1 = hotel.Address1,
                Address2 = hotel.Address2,
                City = hotel.City,
                State = hotel.State,
                ZipCode = hotel.ZipCode,
                LocalPhone = hotel.LocalPhone,
                TollFreePhone = hotel.TollFreePhone,
                Website = hotel.Website,
                Description = hotel.Description,
                CheckIn = hotel.CheckIn,
                CheckOut = hotel.CheckOut
            };
        }

        public async Task<DisclaimerDto?> GetDisclaimerAsync(int hotelId, CancellationToken ct = default)
        {
            var disclaimer = await _disclaimerRepo.GetDisclaimerForHotel(hotelId);
            if (disclaimer == null) return null;

            return new DisclaimerDto
            {
                ReservationDisclaimer = disclaimer.ReservationDisclaimer ?? string.Empty,
                EmailDisclaimer = disclaimer.EmailDisclaimer ?? string.Empty
            };
        }
    }
}
