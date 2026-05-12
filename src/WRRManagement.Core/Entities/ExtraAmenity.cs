namespace WRRManagement.Core.Entities
{
    public class ExtraAmenity
    {
        public int AmenityID { get; set; }
        public int HotelID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal AmenityRate { get; set; }
        public decimal Tax { get; set; }
        public bool ViewRate { get; set; }
        public bool Mandatory { get; set; }
        public bool Visible { get; set; }
        public bool PerDayPerPerson { get; set; }
        public bool PerDay { get; set; }
        public bool PerNightStay { get; set; }
        public bool OneTimeFee { get; set; }
        public bool OneTimeFeePerson { get; set; }
        public bool Discount { get; set; }
        public decimal? DiscountRegularRate { get; set; }
        public string? PictureUrl { get; set; }
        public bool ViewOnRackRate { get; set; }
        public int? MandatoryQty { get; set; }
        public bool AdditionalPurchases { get; set; }
    }
}
