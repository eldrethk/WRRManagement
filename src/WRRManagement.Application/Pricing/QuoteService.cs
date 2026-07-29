using Microsoft.Extensions.Logging;
using WRRManagement.Application.Pricing.Dtos;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Pricing
{
    /// <summary>
    /// Orchestrates a full price quote for a room-only stay, or a stay booked under a package,
    /// plus any selected/mandatory extra amenities. Backs POST /quotes and is reused by
    /// reservation creation so the server never trusts a client-submitted price.
    /// </summary>
    public class QuoteService : IQuoteService
    {
        private readonly IHotelSystemRepository _hotelSystemRepo;
        private readonly IRoomTypeRepository _roomTypeRepo;
        private readonly IAdultBaseRepository _adultBaseRepo;
        private readonly IMaxBaseRepository _maxBaseRepo;
        private readonly IRackRateRepository _rackRateRepo;
        private readonly ITierLevelRepository _tierLevelRepo;
        private readonly IPackageRepository _packageRepo;
        private readonly IPackageRateRepository _packageRateRepo;
        private readonly IExtraAmenityRepository _amenityRepo;
        private readonly IPackageAmenityRepository _packageAmenityRepo;
        private readonly ILogger<QuoteService> _logger;

        public QuoteService(
            IHotelSystemRepository hotelSystemRepo,
            IRoomTypeRepository roomTypeRepo,
            IAdultBaseRepository adultBaseRepo,
            IMaxBaseRepository maxBaseRepo,
            IRackRateRepository rackRateRepo,
            ITierLevelRepository tierLevelRepo,
            IPackageRepository packageRepo,
            IPackageRateRepository packageRateRepo,
            IExtraAmenityRepository amenityRepo,
            IPackageAmenityRepository packageAmenityRepo,
            ILogger<QuoteService> logger)
        {
            _hotelSystemRepo = hotelSystemRepo;
            _roomTypeRepo = roomTypeRepo;
            _adultBaseRepo = adultBaseRepo;
            _maxBaseRepo = maxBaseRepo;
            _rackRateRepo = rackRateRepo;
            _tierLevelRepo = tierLevelRepo;
            _packageRepo = packageRepo;
            _packageRateRepo = packageRateRepo;
            _amenityRepo = amenityRepo;
            _packageAmenityRepo = packageAmenityRepo;
            _logger = logger;
        }

        public async Task<QuoteResponseDto> GetQuoteAsync(QuoteRequestDto request, CancellationToken ct = default)
        {
            if (request.CheckOut <= request.CheckIn)
                throw new ArgumentException("Check-out must be after check-in", nameof(request));

            if (request.Adults < 1)
                throw new ArgumentException("At least one adult is required", nameof(request));

            var system = await _hotelSystemRepo.GetSystemAsync(request.HotelId);
            var room = await _roomTypeRepo.GetByIdAsync(request.RoomTypeId)
                ?? throw new InvalidOperationException($"Room type {request.RoomTypeId} not found");

            Package? package = null;
            if (request.PackageId is int packageId)
            {
                package = await _packageRepo.GetByIdAsync(packageId)
                    ?? throw new InvalidOperationException($"Package {packageId} not found");
            }

            AdultBase? adultBase = room.AdultBase ? await _adultBaseRepo.GetByRoomIDAsync(room.RoomTypeID) : null;
            MaxBase? maxBase = room.MaxBase ? await _maxBaseRepo.GetByRoomID(room.RoomTypeID) : null;
            char tierLevel = await _tierLevelRepo.GetTierForDateAsync(request.HotelId, request.CheckIn);

            var dates = new List<DateTime>();
            var dailyRates = new List<decimal>();

            for (var d = request.CheckIn; d < request.CheckOut; d = d.AddDays(1))
            {
                decimal? rate = package?.PricingType == PackagePricingType.PricePoint
                    ? (await _packageRateRepo.GetRateForDateAsync(room.RoomTypeID, d, package.PackageID))?.Price
                    : (await RackRateForDateAsync(room.RoomTypeID, request.HotelId, d));

                if (rate == null)
                    throw new InvalidOperationException($"No rate available for room {room.RoomTypeID} on {d:d}");

                dates.Add(d);
                dailyRates.Add(rate.Value);
            }

            int totalNights = dates.Count;

            // Apply the package's discount shape directly to the per-night rates so subtotal, tax,
            // fees, and deposit all derive from one consistent set of numbers.
            if (package != null)
            {
                switch (package.PricingType)
                {
                    case PackagePricingType.PercentOff:
                        var factor = 1 - (package.PercentageOff ?? 0) / 100;
                        for (int i = 0; i < dailyRates.Count; i++)
                            dailyRates[i] *= factor;
                        break;

                    case PackagePricingType.NightsFree:
                        int freeNights = Math.Min((int)(package.NumberOfNights ?? 0), Math.Max(totalNights - 1, 0));
                        var cheapestNightIndexes = dailyRates
                            .Select((rate, index) => (rate, index))
                            .OrderBy(x => x.rate)
                            .Take(freeNights)
                            .Select(x => x.index);
                        foreach (var index in cheapestNightIndexes)
                            dailyRates[index] = 0;
                        break;

                    case PackagePricingType.PricePoint:
                        // dailyRates already holds the package's own per-night price.
                        break;
                }
            }

            decimal depositPercentage = package != null
                ? system.DepositPackagePercentage ?? 0
                : system.DepositRoomPercentage ?? 0;
            var depositMethod = package != null ? system.PackageDepositCalAs : system.RoomDepositCalAs;

            var fees = StayFeeCalculator.Calculate(
                system, dates, dailyRates, request.Adults, request.Children, adultBase, maxBase,
                depositMethod, depositPercentage);

            decimal weekendFee = package == null || package.WeekendSurcharge ? fees.WeekendFee : 0;
            decimal resortFee = package == null || package.ResortFees ? fees.ResortFee : 0;
            decimal extraGuestFee = package == null || package.ExtraPersonFee ? fees.ExtraGuestFee : 0;
            decimal allExtraFees = weekendFee + resortFee + extraGuestFee;
            decimal roomTotal = fees.SubTotal + fees.Tax + allExtraFees;

            var amenityLines = await BuildAmenityLinesAsync(request, package, totalNights, ct);
            decimal amenitiesSubTotal = amenityLines.Sum(a => a.TotalCharge);

            return new QuoteResponseDto
            {
                RoomTypeId = request.RoomTypeId,
                PackageId = request.PackageId,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                TotalNights = totalNights,
                TierLevel = tierLevel,
                RateDates = dates,
                DailyRates = dailyRates,
                SubTotal = fees.SubTotal,
                WeekendFee = weekendFee,
                ExtraGuestFee = extraGuestFee,
                ResortFee = resortFee,
                Tax = fees.Tax,
                Amenities = amenityLines,
                AmenitiesSubTotal = amenitiesSubTotal,
                Total = roomTotal + amenitiesSubTotal,
                Deposit = fees.Deposit
            };
        }

        private async Task<decimal?> RackRateForDateAsync(int roomTypeId, int hotelId, DateTime date)
        {
            char tier = await _tierLevelRepo.GetTierForDateAsync(hotelId, date);
            var rate = await _rackRateRepo.GetRateForDateAsync(roomTypeId, date);
            if (rate == null) return null;

            return char.ToUpperInvariant(tier) switch
            {
                'B' => rate.TierBRate,
                'C' => rate.TierCRate,
                _ => rate.TierARate
            };
        }

        private async Task<List<AmenityQuoteLineDto>> BuildAmenityLinesAsync(
            QuoteRequestDto request, Package? package, int totalNights, CancellationToken ct)
        {
            var lines = new List<AmenityQuoteLineDto>();
            var selectedIds = new HashSet<int>();

            foreach (var selection in request.Amenities)
            {
                var amenity = await _amenityRepo.GetByIdAsync(selection.AmenityId);
                if (amenity == null || !amenity.Visible) continue;

                var (mandatory, mandatoryQty) = await ResolveMandatoryAsync(amenity, package);
                lines.Add(ToLine(AmenityPricingCalculator.Calculate(amenity, selection.NumPeople, totalNights, mandatory, mandatoryQty)));
                selectedIds.Add(amenity.AmenityID);
            }

            // Mandatory package amenities must be quoted even if the guest didn't explicitly select them.
            if (package != null)
            {
                var mandatoryAmenities = await _packageAmenityRepo.GetMandatoryAmenitiesAsync(package.PackageID);
                foreach (var amenity in mandatoryAmenities)
                {
                    if (!amenity.Visible || selectedIds.Contains(amenity.AmenityID)) continue;

                    var (mandatory, mandatoryQty) = await ResolveMandatoryAsync(amenity, package);
                    lines.Add(ToLine(AmenityPricingCalculator.Calculate(amenity, mandatoryQty ?? 0, totalNights, mandatory, mandatoryQty)));
                }
            }

            return lines;
        }

        private async Task<(bool Mandatory, int? MandatoryQty)> ResolveMandatoryAsync(ExtraAmenity amenity, Package? package)
        {
            if (package != null)
            {
                var packageAmenity = await _packageAmenityRepo.GetAsync(package.PackageID, amenity.AmenityID);
                if (packageAmenity != null)
                    return (packageAmenity.Mandatory, packageAmenity.MandatoryQuantity ?? amenity.MandatoryQty);
            }

            return (amenity.Mandatory, amenity.MandatoryQty);
        }

        private static AmenityQuoteLineDto ToLine(AmenityChargeResult result) => new()
        {
            AmenityId = result.AmenityId,
            Name = result.Name,
            PricingType = result.PricingType.ToString(),
            NumPeople = result.NumPeople,
            NumNights = result.NumNights,
            ChargeAmount = result.ChargeAmount,
            TaxRate = result.TaxRate,
            Tax = result.Tax,
            TotalCharge = result.TotalCharge,
            Mandatory = result.Mandatory,
            DiscountRegularRate = result.DiscountRegularRate
        };
    }
}
