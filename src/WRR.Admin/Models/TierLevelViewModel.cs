using System.ComponentModel.DataAnnotations;
using WRRManagement.Core.Entities;
using WRR.Admin.Extension;

namespace WRR.Admin.Models
{
    public class TierLevelViewModel :IValidatableObject
    {
        public List<Package>? Packages { get; set; }
        [Required]
        public int SelectedID { get; set; }
        public string? SelectedName { get; set; }

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
        [Required(ErrorMessage = "Tier Level is required")]
        public char TierLevel { get; set; }

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
        }
    }
}
