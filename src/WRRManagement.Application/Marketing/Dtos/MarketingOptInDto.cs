namespace WRRManagement.Application.Marketing.Dtos
{
    public class MarketingOptInDto
    {
        public string Email { get; init; } = string.Empty;
        public int HotelId { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string State { get; init; } = string.Empty;
    }
}
