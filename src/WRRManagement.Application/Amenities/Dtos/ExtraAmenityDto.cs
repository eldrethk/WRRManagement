using WRRManagement.Core.Enums;

namespace WRRManagement.Application.Amenities.Dtos
{
    public class ExtraAmenityDto
    {
        public int AmenityId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string ShortDescription { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal AmenityRate { get; init; }
        public decimal Tax { get; init; }
        public AmenityPricingType PricingType { get; init; }
        public bool ViewRate { get; init; }
        public bool Mandatory { get; init; }
        public int? MandatoryQty { get; init; }
        public decimal? DiscountRegularRate { get; init; }
        public string? PictureUrl { get; init; }
        public bool ViewOnRackRate { get; init; }
        public bool AdditionalPurchases { get; init; }
    }
}
