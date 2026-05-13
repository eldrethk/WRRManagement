using System.ComponentModel.DataAnnotations;
using WRR.Admin.Extension;
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class SearchReportViewModel
    {
        [Required(ErrorMessage = "Start Date is required")]
        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "End Date is required")]
        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        [DateGreaterThan("StartDate", ErrorMessage = "End Date must be greater than Start Date")]
        public DateTime? EndDate { get; set; }
        [Required]
        public int HotelID { get; set; }
        [Display(Name = "Search By")]
        public string SearchBy { get; set; }
    }
}
