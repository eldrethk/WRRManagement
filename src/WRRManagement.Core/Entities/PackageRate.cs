namespace WRRManagement.Core.Entities
{
    public class PackageRate
    {
        public int RateID { get; set; }
        public int PackageID { get; set; }
        public int RoomTypeID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public bool Visible { get; set; }
    }
}
