namespace WRRManagement.Core.Entities
{
    public class PackageAmenity
    {
        public int PackageAmenityID { get; set; }
        public int PackageID { get; set; }
        public int ExtraAmenityID { get; set; }
        public bool ViewRate { get; set; }
        public bool Mandatory { get; set; }
        public int? MandatoryQuantity { get; set; }
        public bool AdditionalPurchases { get; set; }
    }
}
