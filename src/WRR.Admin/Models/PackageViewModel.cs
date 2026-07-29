using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class PackageViewModel : IValidatableObject
    {
        public Package Package { get; set; }
        public List<RoomType>? RoomTypes { get; set; }
        public List<ExtraAmenity>? Amenities { get; set; }

        public UploadImageViewModel? UploadImage { get; set; }

        public string PackageType { get; set; }
        [Required(ErrorMessage = "Rooms are required")]
        public int[] SelectedRoomTypeIds { get; set; }

        public IEnumerable<SelectListItem> PackageRoom
        {
            get { return RoomTypes != null ? new SelectList(RoomTypes, "RoomTypeID", "Name") : new List<SelectListItem>(); }
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PackageType == "Percentage")
            {
                if (Package.PercentageOff == null)
                    yield return new ValidationResult("Percentage Off is required");
            }
            else if(PackageType == "Nights")
            {
                if (Package.NumberOfNights == null || Package.NumberOfNights <= 0)
                    yield return new ValidationResult("Nights free is required");
            }
        }
    }

}
