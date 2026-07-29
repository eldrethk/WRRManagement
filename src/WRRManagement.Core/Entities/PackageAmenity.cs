namespace WRRManagement.Core.Entities
{
    public class PackageAmenity
    {
        public int PackageAmenityID { get; internal set; }
        public int PackageID { get; internal set; }
        public int ExtraAmenityID { get; internal set; }
        public bool ViewRate { get; internal set; }
        public bool Mandatory { get; internal set; }
        public int? MandatoryQuantity { get; internal set; }
        public bool AdditionalPurchases { get; internal set; }

        public PackageAmenity() { }

        public static PackageAmenity Create(
            int packageId,
            int extraAmenityId,
            bool mandatory,
            int? mandatoryQuantity,
            bool additionalPurchases)
        {
            if (packageId <= 0)
                throw new ArgumentException("Package Amenity must be assigned to a valid package", nameof(packageId));

            if (extraAmenityId <= 0)
                throw new ArgumentException("Package Amenity must be assigned to a valid amenity", nameof(extraAmenityId));

            if (mandatory && mandatoryQuantity is < 1)
                throw new ArgumentException("Mandatory quantity must be at least 1 for a mandatory amenity", nameof(mandatoryQuantity));

            return new PackageAmenity
            {
                PackageID = packageId,
                ExtraAmenityID = extraAmenityId,
                Mandatory = mandatory,
                MandatoryQuantity = mandatory ? mandatoryQuantity : null,
                AdditionalPurchases = additionalPurchases
            };
        }

        public void Update(bool mandatory, int? mandatoryQuantity, bool additionalPurchases)
        {
            if (mandatory && mandatoryQuantity is < 1)
                throw new ArgumentException("Mandatory quantity must be at least 1 for a mandatory amenity", nameof(mandatoryQuantity));

            Mandatory = mandatory;
            MandatoryQuantity = mandatory ? mandatoryQuantity : null;
            AdditionalPurchases = additionalPurchases;
        }
    }
}
