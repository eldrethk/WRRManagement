namespace WRRManagement.Core.Entities
{
    public class ReservationQue
    {
        public int QueID { get; set; }
        public int ReservationID { get; set; }
        public DateTime BookedDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ReservationType { get; set; }
        public bool Viewed { get; set; }
        public int HotelID { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class ReservationStats
    {
        public int TotalCount { get; set; }
        public int TotalNight { get; set; }
        public decimal TotalRate { get; set; }
    }

    public class AmenityStats
    {
        public int TotalCount { get; set; }
        public decimal TotalRate { get; set; }
    }
}
