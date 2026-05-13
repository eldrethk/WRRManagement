namespace WRRManagement.Application.Reservations.Dtos
{
    public class CreateReservationDto
    {
        public int HotelId { get; init; }
        public int RoomTypeId { get; init; }
        public int PaymentTypeId { get; init; }
        public DateTime ArrivalDate { get; init; }
        public DateTime DepartureDate { get; init; }
        public int TotalNights { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public decimal AvgDailyRate { get; init; }
        public decimal SubTotal { get; init; }
        public char TierLevel { get; init; }
        public decimal ExtraAdultCharge { get; init; }
        public decimal ExtraChildCharge { get; init; }
        public decimal WeekendFees { get; init; }
        public decimal ResortFees { get; init; }
        public decimal TotalFees { get; init; }
        public decimal Taxes { get; init; }
        public decimal TotalCharge { get; init; }
        public decimal Deposit { get; init; }
        public decimal ExtraFees { get; init; }
        public string? Comments { get; init; }
        public string CardHolderName { get; init; } = string.Empty;
        public string CardExpirationDate { get; init; } = string.Empty;
        public string CardNumber { get; init; } = string.Empty;
        public string CardSecureCode { get; init; } = string.Empty;
        public string CusFirstName { get; init; } = string.Empty;
        public string CusLastName { get; init; } = string.Empty;
        public string CusAddress1 { get; init; } = string.Empty;
        public string? CusAddress2 { get; init; }
        public string CusCity { get; init; } = string.Empty;
        public string CusState { get; init; } = string.Empty;
        public string CusZip { get; init; } = string.Empty;
        public string CusDayPhone { get; init; } = string.Empty;
        public string? CusEveningPhone { get; init; }
        public string CusEmail { get; init; } = string.Empty;
        public string? SessionId { get; init; }
        public int CustomerId { get; init; }
        public IReadOnlyList<CreateReservationAmenityDto> Amenities { get; init; } = [];
        public IReadOnlyList<(DateTime Date, decimal Rate)> DailyRates { get; init; } = [];
    }
}
