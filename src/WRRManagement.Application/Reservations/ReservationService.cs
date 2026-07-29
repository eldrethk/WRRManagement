using Microsoft.Extensions.Logging;
using WRRManagement.Application.Pricing;
using WRRManagement.Application.Pricing.Dtos;
using WRRManagement.Application.Reservations.Dtos;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Reservations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IReservationAmenityRepository _amenityRepo;
        private readonly IRoomAllocationRepository _roomAllocationRepo;
        private readonly IPackageAllocationRepository _packageAllocationRepo;
        private readonly IQuoteService _quoteService;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository reservationRepo,
            IReservationAmenityRepository amenityRepo,
            IRoomAllocationRepository roomAllocationRepo,
            IPackageAllocationRepository packageAllocationRepo,
            IQuoteService quoteService,
            ILogger<ReservationService> logger)
        {
            _reservationRepo = reservationRepo;
            _amenityRepo = amenityRepo;
            _roomAllocationRepo = roomAllocationRepo;
            _packageAllocationRepo = packageAllocationRepo;
            _quoteService = quoteService;
            _logger = logger;
        }

        public async Task<int> CreateAsync(CreateReservationDto dto, Guid? idempotencyKey, CancellationToken ct = default)
        {
            if (idempotencyKey is Guid key)
            {
                var priorReservationId = await _reservationRepo.GetIdByIdempotencyKeyAsync(key);
                if (priorReservationId is int existingId)
                {
                    _logger.LogInformation(
                        "Idempotent replay for key {IdempotencyKey} returned existing reservation {ReservationId}",
                        key, existingId);
                    return existingId;
                }
            }

            if (dto.ArrivalDate >= dto.DepartureDate)
                throw new ArgumentException("Arrival date must be before departure date");

            var roomAvailable = await _roomAllocationRepo.AllocationIsValidAsync(dto.RoomTypeId, dto.ArrivalDate, dto.DepartureDate);
            if (!roomAvailable)
                throw new InvalidOperationException("Room is no longer available for the selected dates");

            if (dto.PackageId is int packageId)
            {
                var packageAvailable = await _packageAllocationRepo.IsValidAsync(dto.RoomTypeId, packageId, dto.ArrivalDate, dto.DepartureDate);
                if (!packageAvailable)
                    throw new InvalidOperationException("Package is no longer available for the selected dates");
            }

            // Pricing is always recomputed here — a client-submitted price is never trusted.
            var quote = await _quoteService.GetQuoteAsync(new QuoteRequestDto
            {
                HotelId = dto.HotelId,
                RoomTypeId = dto.RoomTypeId,
                PackageId = dto.PackageId,
                CheckIn = dto.ArrivalDate,
                CheckOut = dto.DepartureDate,
                Adults = dto.Adults,
                Children = dto.Children,
                Amenities = dto.Amenities
            }, ct);

            var reservation = new Reservation
            {
                HotelID = dto.HotelId,
                RoomTypeID = dto.RoomTypeId,
                PackageID = dto.PackageId,
                PaymentTypeID = dto.PaymentTypeId,
                ArrivalDate = dto.ArrivalDate,
                DepartureDate = dto.DepartureDate,
                TotalNights = quote.TotalNights,
                Adults = dto.Adults,
                Children = dto.Children,
                AvgDailyRate = quote.TotalNights > 0 ? quote.SubTotal / quote.TotalNights : 0,
                SubTotal = quote.SubTotal,
                TierLevel = quote.TierLevel,
                ExtraAdultCharge = quote.ExtraGuestFee,
                ExtraChildCharge = 0,
                WeekendFees = quote.WeekendFee,
                ResortFees = quote.ResortFee,
                TotalFees = quote.WeekendFee + quote.ResortFee + quote.ExtraGuestFee,
                Taxes = quote.Tax,
                TotalCharge = quote.Total,
                Deposit = quote.Deposit,
                ExtraFees = quote.AmenitiesSubTotal,
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
                BookedAmenity = quote.Amenities.Count > 0,
                UserInitials = "WEB",
                ReservationCreated = DateTime.UtcNow,
                SessionID = dto.SessionId,
                CustomerId = dto.CustomerId,
                IdempotencyKey = idempotencyKey
            };

            int reservationId;
            try
            {
                reservationId = await _reservationRepo.CreateAsync(reservation);
            }
            catch (Exception) when (idempotencyKey is not null)
            {
                // A concurrent retry with the same key may have raced us to the unique index —
                // return the winner's reservation instead of surfacing a duplicate-key error.
                var winnerId = await _reservationRepo.GetIdByIdempotencyKeyAsync(idempotencyKey.Value);
                if (winnerId is int id)
                {
                    _logger.LogInformation(
                        "Reservation insert for idempotency key {IdempotencyKey} lost a race; returning existing reservation {ReservationId}",
                        idempotencyKey, id);
                    return id;
                }
                throw;
            }

            if (quote.RateDates.Count > 0)
            {
                var dailyRates = quote.RateDates.Zip(quote.DailyRates, (date, rate) => (date, rate));
                await _reservationRepo.AddDailyRatesAsync(reservationId, dailyRates);
            }

            foreach (var amenity in quote.Amenities)
            {
                await _amenityRepo.AddAsync(ReservationAmenity.Create(
                    reservationId,
                    amenity.AmenityId,
                    amenity.ChargeAmount,
                    amenity.Tax,
                    amenity.Mandatory,
                    amenity.TaxRate,
                    amenity.NumPeople,
                    dto.ArrivalDate,
                    amenity.TotalCharge));
            }

            _logger.LogInformation("Created reservation {ReservationId} for hotel {HotelId}", reservationId, dto.HotelId);
            return reservationId;
        }
    }
}
