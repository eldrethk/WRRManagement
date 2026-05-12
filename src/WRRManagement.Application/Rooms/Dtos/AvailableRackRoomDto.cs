namespace WRRManagement.Application.Rooms.Dtos
{
    public class AvailableRackRoomDto
    {
        public int RoomTypeId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? BedType { get; init; }
        public string? MainImageUrl { get; init; }
        public IReadOnlyList<string> Features { get; init; } = [];
        public int MaxGuests { get; init; }

        // Pricing — empty when MinStay > 0
        public IReadOnlyList<DateTime> RateDates { get; init; } = [];
        public IReadOnlyList<decimal> DailyRates { get; init; } = [];
        public decimal SubTotal { get; init; }
        public decimal WeekendFee { get; init; }
        public decimal ExtraGuestFee { get; init; }
        public decimal Tax { get; init; }
        public decimal ResortFee { get; init; }
        public decimal AllExtraFees { get; init; }
        public decimal AvgDailyRate { get; init; }
        public decimal Total { get; init; }
        public decimal Deposit { get; init; }

        // > 0 when the stay length is less than the minimum required nights
        public int MinStay { get; init; }

        // > 0 (and <= hotel's low-allocation limit) when rooms are running low
        public int LowAllocation { get; init; }

        public string RateDisplayAs { get; init; } = string.Empty;
    }
}
