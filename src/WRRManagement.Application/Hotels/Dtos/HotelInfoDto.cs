namespace WRRManagement.Application.Hotels.Dtos
{
    public class HotelInfoDto
    {
        public int HotelId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string ZipCode { get; init; } = string.Empty;
        public string LocalPhone { get; init; } = string.Empty;
        public string? TollFreePhone { get; init; }
        public string CheckInTime { get; init; } = string.Empty;
        public string CheckOutTime { get; init; } = string.Empty;
    }
}
