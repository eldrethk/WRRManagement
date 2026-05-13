using Microsoft.Extensions.Logging;
using WRRManagement.Application.Reservations.Dtos;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Reservations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IReservationAmenityRepository _amenityRepo;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository reservationRepo,
            IReservationAmenityRepository amenityRepo,
            ILogger<ReservationService> logger)
        {
            _reservationRepo = reservationRepo;
            _amenityRepo = amenityRepo;
            _logger = logger;
        }

        public async Task<int> CreateAsync(CreateReservationDto dto, CancellationToken ct = default)
        {
            var reservation = new Reservation
            {
                HotelID = dto.HotelId,
                RoomTypeID = dto.RoomTypeId,
                PaymentTypeID = dto.PaymentTypeId,
                ArrivalDate = dto.ArrivalDate,
                DepartureDate = dto.DepartureDate,
                TotalNights = dto.TotalNights,
                Adults = dto.Adults,
                Children = dto.Children,
                AvgDailyRate = dto.AvgDailyRate,
                SubTotal = dto.SubTotal,
                TierLevel = dto.TierLevel,
                ExtraAdultCharge = dto.ExtraAdultCharge,
                ExtraChildCharge = dto.ExtraChildCharge,
                WeekendFees = dto.WeekendFees,
                ResortFees = dto.ResortFees,
                TotalFees = dto.TotalFees,
                Taxes = dto.Taxes,
                TotalCharge = dto.TotalCharge,
                Deposit = dto.Deposit,
                ExtraFees = dto.ExtraFees,
                Comments = dto.Comments,
                CardHolderName = dto.CardHolderName,
                CardExpirationDate = dto.CardExpirationDate,
                CardNumber = dto.CardNumber,
                CardSecureCode = dto.CardSecureCode,
                CusFirstName = dto.CusFirstName,
                CusLastName = dto.CusLastName,
                CusAddress1 = dto.CusAddress1,
                CusAddress2 = dto.CusAddress2,
                CusCity = dto.CusCity,
                CusState = dto.CusState,
                CusZip = dto.CusZip,
                CusDayPhone = dto.CusDayPhone,
                CusEveningPhone = dto.CusEveningPhone,
                CusEmail = dto.CusEmail,
                BookedAmenity = dto.Amenities.Count > 0,
                UserInitials = "WEB",
                ReservationCreated = DateTime.UtcNow,
                SessionID = dto.SessionId,
                CustomerId = dto.CustomerId
            };

            var reservationId = await _reservationRepo.CreateAsync(reservation);

            if (dto.DailyRates.Count > 0)
                await _reservationRepo.AddDailyRatesAsync(reservationId, dto.DailyRates);

            foreach (var amenity in dto.Amenities)
            {
                await _amenityRepo.AddAsync(new ReservationAmenity
                {
                    ReservationID = reservationId,
                    AmenityID = amenity.AmenityId,
                    ChargeAmount = amenity.ChargeAmount,
                    TaxIncluded = amenity.TaxIncluded,
                    Mandatory = amenity.Mandatory,
                    TaxRate = amenity.TaxRate,
                    NumPeople = amenity.NumPeople,
                    NumDate = amenity.NumDate,
                    TotalCharge = amenity.TotalCharge
                });
            }

            _logger.LogInformation("Created reservation {ReservationId} for hotel {HotelId}", reservationId, dto.HotelId);
            return reservationId;
        }
    }
}
