namespace WRRManagement.Core.Entities
{
    public class CalendarMessage
    {
        public int MessageID { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DisplayFrom { get; set; }
        public DateTime DisplayTo { get; set; }
        public int HotelID { get; set; }
    }
}
