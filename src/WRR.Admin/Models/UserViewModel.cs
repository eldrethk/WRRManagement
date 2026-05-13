using Microsoft.AspNetCore.Mvc.Rendering;
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class UserViewModel
    {
        public List<Hotel> Hotels { get; set; }
        public string UserID { get; set; }

        public string SelectedHotel { get; set; } 
        public IEnumerable<SelectListItem> UserHotels
        {
            get { return Hotels != null ? new SelectList(Hotels, "HotelID", "Name") : new List<SelectListItem>(); }
        }

    }
}
