using System.ComponentModel.DataAnnotations;
using WRR.Admin.Extension;
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class DateRangeViewModel
    {
        public int SelectedRoomTypeID { get; set; }
        public string? SelectRoomName { get; set; }
        [Display(Name = "Start Date")]
        [Required(ErrorMessage = "Start Date is required")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        [Required(ErrorMessage = "End Date is required")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        [DateGreaterThan("StartDate", ErrorMessage = "End Date must be greater than Start Date")]
        public DateTime EndDate { get; set; }

        public string? Message { get; set; }
    }
}
