using WRRManagement.Core.Enums;

namespace WRRManagement.Core.Entities
{
    public class ExtraAmenity
    {
        public int AmenityID { get; internal set; }
        public int HotelID { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public string ShortDescription { get; internal set; } = string.Empty;
        public string Description { get; internal set; } = string.Empty;
        public decimal AmenityRate { get; internal set; }
        public decimal Tax { get; internal set; }
        public AmenityPricingType PricingType { get; internal set; }
        public bool ViewRate { get; internal set; }
        public bool Mandatory { get; internal set; }
        public int? MandatoryQty { get; internal set; }
        public bool Visible { get; internal set; }
        public decimal? DiscountRegularRate { get; internal set; }
        public string? PictureUrl { get; internal set; }
        public bool ViewOnRackRate { get; internal set; }
        public bool AdditionalPurchases { get; internal set; }

        public ExtraAmenity() { }

        public static ExtraAmenity Create(
            int hotelId,
            string name,
            string shortDescription,
            string description,
            decimal amenityRate,
            decimal tax,
            AmenityPricingType pricingType,
            bool viewOnRackRate,
            bool mandatory,
            int? mandatoryQty,
            decimal? discountRegularRate,
            string? pictureUrl,
            bool additionalPurchases)
        {
            if (hotelId <= 0)
                throw new ArgumentException("Amenity must be assigned to a valid hotel", nameof(hotelId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Amenity name is required", nameof(name));

            if (amenityRate < 0)
                throw new ArgumentException("Amenity rate can not be negative", nameof(amenityRate));

            if (tax < 0)
                throw new ArgumentException("Tax can not be negative", nameof(tax));

            if (pricingType == AmenityPricingType.Discount && discountRegularRate == null)
                throw new ArgumentException("Original rate is required for a discount amenity", nameof(discountRegularRate));

            return new ExtraAmenity
            {
                HotelID = hotelId,
                Name = name,
                ShortDescription = shortDescription,
                Description = description,
                AmenityRate = amenityRate,
                Tax = tax,
                PricingType = pricingType,
                ViewOnRackRate = viewOnRackRate,
                Mandatory = mandatory,
                MandatoryQty = mandatoryQty,
                DiscountRegularRate = pricingType == AmenityPricingType.Discount ? discountRegularRate : null,
                PictureUrl = pictureUrl,
                AdditionalPurchases = additionalPurchases,
                Visible = true
            };
        }

        public void Update(
            string name,
            string shortDescription,
            string description,
            decimal amenityRate,
            decimal tax,
            AmenityPricingType pricingType,
            bool viewOnRackRate,
            bool mandatory,
            int? mandatoryQty,
            decimal? discountRegularRate,
            string? pictureUrl,
            bool additionalPurchases)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Amenity name is required", nameof(name));

            if (amenityRate < 0)
                throw new ArgumentException("Amenity rate can not be negative", nameof(amenityRate));

            if (tax < 0)
                throw new ArgumentException("Tax can not be negative", nameof(tax));

            if (pricingType == AmenityPricingType.Discount && discountRegularRate == null)
                throw new ArgumentException("Original rate is required for a discount amenity", nameof(discountRegularRate));

            Name = name;
            ShortDescription = shortDescription;
            Description = description;
            AmenityRate = amenityRate;
            Tax = tax;
            PricingType = pricingType;
            ViewOnRackRate = viewOnRackRate;
            Mandatory = mandatory;
            MandatoryQty = mandatoryQty;
            DiscountRegularRate = pricingType == AmenityPricingType.Discount ? discountRegularRate : null;
            PictureUrl = pictureUrl;
            AdditionalPurchases = additionalPurchases;
        }

        public void SetVisible(bool visible) => Visible = visible;
    }
}
