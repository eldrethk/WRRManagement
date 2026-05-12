namespace WRRManagement.Core.Entities
{
    public class OptInEmails
    {
        public int ID { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public int HotelID { get; set; }
        public DateTime OptInDate { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? State { get; set; }
    }
}
