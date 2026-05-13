using System.ComponentModel.DataAnnotations;
using WRR.Admin.Extension;
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class RoomEventViewModel : IValidatableObject
    {
        public List<RoomType>? Rooms { get; set; }
        [Required(ErrorMessage = "Please Select a Room Type")]
        public int SelectedRoomTypeID { get; set; }
        public string? SelectRoomName { get; set; }

        public int HotelID { get; set; }
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            DateTime temp;
            if (!DateTime.TryParse(StartDate.ToString(), out temp) && !DateTime.TryParse(EndDate.ToString(), out temp))
            {
                yield return new ValidationResult("Please enter a valid date");
            }
            if (StartDate < DateTime.Today || EndDate < DateTime.Today)
            {
                yield return new ValidationResult("Date must not be in the past.");

            }
            if(SelectedRoomTypeID <= 0)
            {
                yield return new ValidationResult("Select a Room Type is required");
            }

        }
    }
}
