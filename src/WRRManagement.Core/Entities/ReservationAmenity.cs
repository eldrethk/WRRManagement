namespace WRRManagement.Core.Entities
{
    public class ReservationAmenity
    {
        public int ID { get; set; }
        public int ReservationID { get; set; }
        public int AmenityID { get; set; }
        public decimal ChargeAmount { get; set; }
        public bool TaxIncluded { get; set; }
        public bool Mandatory { get; set; }
        public decimal TaxRate { get; set; }
        public int NumPeople { get; set; }
        public DateTime NumDate { get; set; }
        public decimal TotalCharge { get; set; }
    }
}
