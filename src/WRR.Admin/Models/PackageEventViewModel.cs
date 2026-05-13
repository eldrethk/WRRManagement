using System.ComponentModel.DataAnnotations;
using WRR.Admin.Extension;
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class PackageEventViewModel
    {
        public List<Package>? Packages { get; set; }

        public int SelectedPackageID { get; set; }
        public List<RoomType>? Rooms { get; set; }
        [Required(ErrorMessage = "Please Select a Room Type")]
        public int SelectedRoomTypeID { get; set; }
        public string? SelectRoomName { get; set; }
        [Display(Name = "Start Date")]
        [Required(ErrorMessage = "Start Date is required")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        [Required(ErrorMessage = "End Date is required")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [DateGreaterThan("StartDate", ErrorMessage = "End Date must be greater than start date")]
        public DateTime? EndDate { get; set; }
        [Required(ErrorMessage = "Quantity required")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid integer number")]
        [RegularExpression("([0-9]+)", ErrorMessage = "Please enter valid Number")]
        public int? Quantity { get; set; }
    }
}
