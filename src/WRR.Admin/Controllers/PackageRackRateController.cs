using Microsoft.AspNetCore.Mvc;
using WRR.Admin.Extension;
using WRR.Admin.Models;

namespace WRR.Admin.Controllers
{
    public class PackageRackRateController : Controller
    {
        public IActionResult Index()
        {
            int HotelID = HttpContext.Session.GetInt("HotelID");
            HotelViewModel model = new HotelViewModel
            {
                HotelId = HotelID
            };
            return View(model);
        }
    }
}
