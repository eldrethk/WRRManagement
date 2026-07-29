namespace WRRManagement.Application.Pricing.Dtos
{
    public class QuoteRequestDto
    {
        public int HotelId { get; init; }
        public int RoomTypeId { get; init; }
        public int? PackageId { get; init; }
        public DateTime CheckIn { get; init; }
        public DateTime CheckOut { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public IReadOnlyList<AmenitySelectionDto> Amenities { get; init; } = [];
    }

    public class AmenitySelectionDto
    {
        public int AmenityId { get; init; }
        public int NumPeople { get; init; }
    }
}
