namespace WRRManagement.Application.Hotels.Dtos
{
    public class HotelInfoDto
    {
        public int HotelId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string AdminEmail { get; init; } = string.Empty;
        public string Address1 { get; init; } = string.Empty;
        public string Address2 { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
        public string ZipCode { get; init; } = string.Empty;
        public string LocalPhone { get; init; } = string.Empty;
        public string? TollFreePhone { get; init; }
        public string Website { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string CheckIn { get; init; } = string.Empty;
        public string CheckOut { get; init; } = string.Empty;
    }
}
