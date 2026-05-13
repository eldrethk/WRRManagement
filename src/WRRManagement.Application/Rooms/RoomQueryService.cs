using Microsoft.Extensions.Logging;
using WRRManagement.Application.Rooms.Dtos;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Rooms
{
    public class RoomQueryService : IRoomQueryService
    {
        private readonly IRoomTypeRepository _roomTypeRepo;
        private readonly IRoomImageRepository _roomImageRepo;
        private readonly IRoomFeaturesRepository _roomFeaturesRepo;
        private readonly IAdultBaseRepository _adultBaseRepo;
        private readonly IMaxBaseRepository _maxBaseRepo;
        private readonly IRoomAllocationRepository _roomAllocationRepo;
        private readonly IRackRateRepository _rackRateRepo;
        private readonly ITierLevelRepository _tierLevelRepo;
        private readonly IMinStayRepository _minStayRepo;
        private readonly IHotelSystemRepository _hotelSystemRepo;
        private readonly ILogger<RoomQueryService> _logger;

        public RoomQueryService(
            IRoomTypeRepository roomTypeRepo,
            IRoomImageRepository roomImageRepo,
            IRoomFeaturesRepository roomFeaturesRepo,
            IAdultBaseRepository adultBaseRepo,
            IMaxBaseRepository maxBaseRepo,
            IRoomAllocationRepository roomAllocationRepo,
            IRackRateRepository rackRateRepo,
            ITierLevelRepository tierLevelRepo,
            IMinStayRepository minStayRepo,
            IHotelSystemRepository hotelSystemRepo,
            ILogger<RoomQueryService> logger)
        {
            _roomTypeRepo = roomTypeRepo;
            _roomImageRepo = roomImageRepo;
            _roomFeaturesRepo = roomFeaturesRepo;
            _adultBaseRepo = adultBaseRepo;
            _maxBaseRepo = maxBaseRepo;
            _roomAllocationRepo = roomAllocationRepo;
            _rackRateRepo = rackRateRepo;
            _tierLevelRepo = tierLevelRepo;
            _minStayRepo = minStayRepo;
            _hotelSystemRepo = hotelSystemRepo;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ViewRoomDto>> GetBookableRoomsAsync(int hotelId, CancellationToken ct = default)
        {
            var rooms = await _roomTypeRepo.GetAllForHotelAsync(hotelId);
            var result = new List<ViewRoomDto>();

            foreach (var room in rooms)
            {
                var images = (await _roomImageRepo.GetAllImageForRoomAsync(room.RoomTypeID)).ToList();
                var mainImage = await _roomImageRepo.GetMainForRoomAsync(room.RoomTypeID);
                var features = await _roomFeaturesRepo.GetRoomFeaturesAsync(room.RoomTypeID);

                int maxGuests = await GetMaxGuestsAsync(room);

                result.Add(new ViewRoomDto
                {
                    RoomTypeId = room.RoomTypeID,
                    Name = room.Name,
                    Description = room.Description,
                    BedType = room.BedType,
                    MainImageUrl = mainImage?.BlobUrl,
                    Images = images.Select(i => new RoomImageDto
                    {
                        Id = i.ImageID,
                        BlobUrl = i.BlobUrl,
                        Description = i.Description,
                        SortOrder = i.SortOrder
                    }).ToList(),
                    Features = features.Select(f => new RoomFeatureDto
                    {
                        Id = f.ID,
                        Icon = f.Icon,
                        Feature = f.Features
                    }).ToList(),
                    MaxGuests = maxGuests
                });
            }

            return result;
        }

        public async Task<IReadOnlyList<AvailableRackRoomDto>> SearchAvailabilityAsync(
            int hotelId, DateTime checkIn, DateTime checkOut, int adults, int children, CancellationToken ct = default)
        {
            var system = await _hotelSystemRepo.GetSystemAsync(hotelId);
            var rooms = await _roomTypeRepo.GetAllForHotelAsync(hotelId);
            int totalGuests = adults + children;
            int days = (checkOut - checkIn).Days;
            var results = new List<AvailableRackRoomDto>();

            foreach (var room in rooms)
            {
                // Load capacity data once — reused for filtering and fee computation
                AdultBase? adultBase = null;
                MaxBase? maxBase = null;
                int maxGuests;

                if (room.AdultBase)
                {
                    adultBase = await _adultBaseRepo.GetByRoomIDAsync(room.RoomTypeID);
                    if (adultBase == null
                        || totalGuests > adultBase.MaxRoomTotal
                        || adults > adultBase.MaxAdult
                        || children > adultBase.MaxChild)
                        continue;
                    maxGuests = adultBase.MaxRoomTotal;
                }
                else if (room.MaxBase)
                {
                    maxBase = await _maxBaseRepo.GetByRoomID(room.RoomTypeID);
                    if (maxBase == null || totalGuests > maxBase.MaxBaseCount)
                        continue;
                    maxGuests = maxBase.MaxBaseCount;
                }
                else
                {
                    if (totalGuests > 2) continue;
                    maxGuests = 2;
                }

                // Allocation check
                bool hasAllocation = await _roomAllocationRepo.AllocationIsValidAsync(room.RoomTypeID, checkIn, checkOut);
                if (!hasAllocation) continue;

                // Min-stay check — find the highest min-stay requirement across all dates
                int requiredMinStay = 0;
                for (var d = checkIn; d < checkOut; d = d.AddDays(1))
                {
                    int stay = await _minStayRepo.GetQuantityForDateAsync(room.RoomTypeID, d);
                    if (stay > requiredMinStay) requiredMinStay = stay;
                }

                if (requiredMinStay > 0 && days < requiredMinStay)
                {
                    results.Add(new AvailableRackRoomDto
                    {
                        RoomTypeId = room.RoomTypeID,
                        Name = room.Name,
                        BedType = room.BedType,
                        MinStay = requiredMinStay
                    });
                    continue;
                }

                // Rate computation
                var rateDates = new List<DateTime>();
                var dailyRates = new List<decimal>();
                decimal subTotal = 0;
                bool ratesAvailable = true;

                for (var d = checkIn; d < checkOut; d = d.AddDays(1))
                {
                    char tier = await _tierLevelRepo.GetTierForDateAsync(hotelId, d);
                    var rate = await _rackRateRepo.GetRateForDateAsync(room.RoomTypeID, d);

                    if (rate == null)
                    {
                        _logger.LogWarning("No rack rate for room {RoomTypeId} on {Date}", room.RoomTypeID, d);
                        ratesAvailable = false;
                        break;
                    }

                    decimal dayRate = char.ToUpperInvariant(tier) switch
                    {
                        'B' => rate.TierBRate,
                        'C' => rate.TierCRate,
                        _ => rate.TierARate
                    };

                    rateDates.Add(d);
                    dailyRates.Add(dayRate);
                    subTotal += dayRate;
                }

                if (!ratesAvailable) continue;

                // Weekend fee
                decimal weekendFee = 0;
                if (system.WeekendFee > 0)
                {
                    foreach (var d in rateDates)
                    {
                        if (d.DayOfWeek == DayOfWeek.Friday || d.DayOfWeek == DayOfWeek.Saturday)
                            weekendFee += system.WeekendFee;
                    }
                    if (system.AddTaxToWeekendFee)
                        weekendFee *= 1 + system.TaxRate / 100;
                }

                // Extra guest fee
                decimal extraGuestFee = 0;
                if (maxBase != null)
                {
                    int over = totalGuests - maxBase.MaxBaseCount;
                    if (over > 0) extraGuestFee = over * system.ExtraBaseFee * days;
                }
                else if (adultBase != null)
                {
                    int adultOver = adults - adultBase.AdultBaseCount;
                    int childOver = children - adultBase.ChildBaseCount;
                    if (adultOver > 0) extraGuestFee += adultOver * system.ExtraAdultFee * days;
                    if (childOver > 0) extraGuestFee += childOver * system.ExtraChildFee * days;
                }
                if (system.AddTaxToExtraPerson && extraGuestFee > 0)
                    extraGuestFee *= 1 + system.TaxRate / 100;

                decimal taxRate = system.TaxRate / 100;
                decimal tax = subTotal * taxRate;

                // Resort fee
                decimal resortFee = system.HotelResortFeeCalAs switch
                {
                    ResortFeeCalculationMethod.FlatFee => system.ResortFee,
                    ResortFeeCalculationMethod.FlatFeePerPerson => system.ResortFee * totalGuests,
                    _ => system.ResortFee * days // FlatFeePerDay (default)
                };
                if (system.AddTaxToResortFee)
                    resortFee *= 1 + taxRate;

                decimal allExtraFees = resortFee + extraGuestFee + weekendFee;
                decimal total = subTotal + tax + allExtraFees;

                // Deposit
                decimal deposit = system.RoomDepositCalAs switch
                {
                    DepositCalculationMethod.FirstTwoNightsRoomStay =>
                        (dailyRates.Count >= 2 ? dailyRates[0] + dailyRates[1] : dailyRates[0])
                        * (system.AddTaxToDeposit ? 1 + taxRate : 1),
                    DepositCalculationMethod.PercentageOfTotal => system.DepositRoomPercentage ?? 0,
                    DepositCalculationMethod.TotalReservation => total,
                    _ => dailyRates[0] * (system.AddTaxToDeposit ? 1 + taxRate : 1) // FirstNightRoomStay
                };

                // Low allocation warning
                int lowAllocation = await _roomAllocationRepo.LowestAllocationAsync(room.RoomTypeID, checkIn, checkOut);
                int lowAllocationDisplay = lowAllocation <= system.LowAllocationLimit ? lowAllocation : 0;

                var mainImage = await _roomImageRepo.GetMainForRoomAsync(room.RoomTypeID);
                var features = await _roomFeaturesRepo.GetRoomFeaturesAsync(room.RoomTypeID);

                results.Add(new AvailableRackRoomDto
                {
                    RoomTypeId = room.RoomTypeID,
                    Name = room.Name,
                    BedType = room.BedType,
                    MainImageUrl = mainImage?.BlobUrl,
                    Features = features.Select(f => f.Features).ToList(),
                    MaxGuests = maxGuests,
                    RateDates = rateDates,
                    DailyRates = dailyRates,
                    SubTotal = subTotal,
                    WeekendFee = weekendFee,
                    ExtraGuestFee = extraGuestFee,
                    Tax = tax,
                    ResortFee = resortFee,
                    AllExtraFees = allExtraFees,
                    AvgDailyRate = days > 0 ? subTotal / days : 0,
                    Total = total,
                    Deposit = deposit,
                    LowAllocation = lowAllocationDisplay,
                    RateDisplayAs = system.RoomRateBreakdownAs.ToString()
                });
            }

            return results;
        }

        public async Task<bool> IsRoomAvailableAsync(int roomTypeId, DateTime checkIn, DateTime checkOut, CancellationToken ct = default)
        {
            try
            {
                return await _roomAllocationRepo.AllocationIsValidAsync(roomTypeId, checkIn, checkOut);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Availability check failed for room {RoomTypeId} [{CheckIn} - {CheckOut}]",
                    roomTypeId, checkIn, checkOut);
                return false;
            }
        }

        private async Task<int> GetMaxGuestsAsync(RoomType room)
        {
            if (room.AdultBase)
            {
                var ab = await _adultBaseRepo.GetByRoomIDAsync(room.RoomTypeID);
                return ab?.MaxRoomTotal ?? 2;
            }
            if (room.MaxBase)
            {
                var mb = await _maxBaseRepo.GetByRoomID(room.RoomTypeID);
                return mb?.MaxBaseCount ?? 2;
            }
            return 2;
        }
    }
}
