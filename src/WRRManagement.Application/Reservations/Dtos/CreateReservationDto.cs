using WRRManagement.Application.Pricing.Dtos;

namespace WRRManagement.Application.Reservations.Dtos
{
    /// <summary>
    /// Guest-selection input only — no price fields. ReservationService recomputes every price
    /// via IQuoteService server-side; a client-submitted total is never trusted.
    /// </summary>
    public class CreateReservationDto
    {
        public int HotelId { get; init; }
        public int RoomTypeId { get; init; }
        public int? PackageId { get; init; }
        public int PaymentTypeId { get; init; }
        public DateTime ArrivalDate { get; init; }
        public DateTime DepartureDate { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public IReadOnlyList<AmenitySelectionDto> Amenities { get; init; } = [];
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
    }
}
