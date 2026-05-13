using System.ComponentModel.DataAnnotations;
using WRRManagement.Core.Interfaces;
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class AmenityViewModel :IValidatableObject
    {

        public ExtraAmenity Amenity { get; set; }
        
        //public string Description { get; set; }
        public string AmenityType { get; set; }
        public bool ViewRate { get; set; }
        public bool ViewOnRackRate { get; set; }
        public List<Package>? Packages { get; set; }

        public UploadImageViewModel? UploadImage { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AmenityType == "Discount")
            {
                Amenity.Discount = true;
                if (Amenity.DiscountRegularRate == null)
                    yield return new ValidationResult("Original Rate is required for discount");
            }
        }

    }
}
