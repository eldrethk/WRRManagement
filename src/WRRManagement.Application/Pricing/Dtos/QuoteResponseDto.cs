namespace WRRManagement.Application.Pricing.Dtos
{
    public class QuoteResponseDto
    {
        public int RoomTypeId { get; init; }
        public int? PackageId { get; init; }
        public DateTime CheckIn { get; init; }
        public DateTime CheckOut { get; init; }
        public int TotalNights { get; init; }
        public char TierLevel { get; init; }
        public IReadOnlyList<DateTime> RateDates { get; init; } = [];
        public IReadOnlyList<decimal> DailyRates { get; init; } = [];
        public decimal SubTotal { get; init; }
        public decimal WeekendFee { get; init; }
        public decimal ExtraGuestFee { get; init; }
        public decimal ResortFee { get; init; }
        public decimal Tax { get; init; }
        public IReadOnlyList<AmenityQuoteLineDto> Amenities { get; init; } = [];
        public decimal AmenitiesSubTotal { get; init; }
        public decimal Total { get; init; }
        public decimal Deposit { get; init; }
    }

    public class AmenityQuoteLineDto
    {
        public int AmenityId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string PricingType { get; init; } = string.Empty;
        public int NumPeople { get; init; }
        public int NumNights { get; init; }
        public decimal ChargeAmount { get; init; }
        public decimal TaxRate { get; init; }
        public decimal Tax { get; init; }
        public decimal TotalCharge { get; init; }
        public bool Mandatory { get; init; }
        public decimal? DiscountRegularRate { get; init; }
    }
}
