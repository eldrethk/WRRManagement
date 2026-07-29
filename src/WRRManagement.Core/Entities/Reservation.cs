namespace WRRManagement.Core.Entities
{
    public class Reservation
    {
        public int ReservationID { get; set; }
        public int HotelID { get; set; }
        public int RoomTypeID { get; set; }
        public int? PackageID { get; set; }
        public int PaymentTypeID { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public int TotalNights { get; set; }
        public int Adults { get; set; }
        public int Children { get; set; }
        public decimal AvgDailyRate { get; set; }
        public decimal SubTotal { get; set; }
        public char TierLevel { get; set; }
        public decimal ExtraAdultCharge { get; set; }
        public decimal ExtraChildCharge { get; set; }
        public decimal WeekendFees { get; set; }
        public decimal ResortFees { get; set; }
        public decimal TotalFees { get; set; }
        public decimal Taxes { get; set; }
        public decimal TotalCharge { get; set; }
        public decimal Deposit { get; set; }
        public decimal ExtraFees { get; set; }
        public string? Comments { get; set; }
        public string CardHolderName { get; set; } = string.Empty;
        public string CardExpirationDate { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string CardSecureCode { get; set; } = string.Empty;
        public string CusFirstName { get; set; } = string.Empty;
        public string CusLastName { get; set; } = string.Empty;
        public string CusAddress1 { get; set; } = string.Empty;
        public string? CusAddress2 { get; set; }
        public string CusCity { get; set; } = string.Empty;
        public string CusState { get; set; } = string.Empty;
        public string CusZip { get; set; } = string.Empty;
        public string CusDayPhone { get; set; } = string.Empty;
        public string? CusEveningPhone { get; set; }
        public string CusEmail { get; set; } = string.Empty;
        public bool BookedAmenity { get; set; }
        public string UserInitials { get; set; } = string.Empty;
        public DateTime ReservationCreated { get; set; }
        public string? SessionID { get; set; }
        public int CustomerId { get; set; }
        public Guid? IdempotencyKey { get; set; }
    }
}
