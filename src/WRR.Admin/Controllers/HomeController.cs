using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using WRRManagement.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WRR.Admin.Extension;
using WRR.Admin.Models;
using X.PagedList.Extensions;


namespace WRR.Admin.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHotelUserRepository hotelUserRep;
        private readonly IHotelRepository hotelRep;
        private readonly IDashboardRepository dashboardRep;

        public HomeController(ILogger<HomeController> logger, IHotelUserRepository hotelUser,
            IHotelRepository hotelRep, IDashboardRepository dashboardRep)
        {
            _logger = logger;
            this.hotelUserRep = hotelUser;
            this.hotelRep = hotelRep;
            this.dashboardRep = dashboardRep;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard(int page = 1)
        {
            int hotelId = HttpContext.Session.GetInt("HotelID");

            if (hotelId <= 0)
                return RedirectToAction("MenuList");

            var hotel = await hotelRep.GetByIdAsync(hotelId);
            if (hotel != null)
                HttpContext.Session.SetString("HotelName", hotel.Name);

            var queue     = await dashboardRep.GetReservationQueAsync(hotelId);
            var rackStats = await dashboardRep.GetRackRateStatsAsync(hotelId);
            var pkgStats  = await dashboardRep.GetPackageStatsAsync(hotelId);
            var amenStats = await dashboardRep.GetAmenityStatsAsync(hotelId);
            var arrivals  = await dashboardRep.GetTodaysArrivalsAsync(hotelId);
            var departs   = await dashboardRep.GetTodaysDeparturesAsync(hotelId);
            var occupied  = await dashboardRep.GetOccupiedRoomsAsync(hotelId);

            var model = new DashboardViewModel
            {
                Hotel            = hotel,
                ReservationQue   = queue.ToPagedList(page, 10),
                Reservation_Booked = rackStats,
                Specials_Booked  = pkgStats,
                Amenitity_Booked = amenStats,
                TodaysArrival    = arrivals,
                TodaysDeparture  = departs,
                RoomOccuied      = occupied
            };

            return View(model);
        }

        public async Task<IActionResult> MenuList()
        {
            try
            {
                var UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var hotels = await hotelUserRep.GetHotelsForUser(UserID);
                var model = new UserViewModel()
                {
                    Hotels = hotels.ToList(),
                    UserID = UserID
                };
                return View(model);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        public IActionResult SetHotel(UserViewModel model)
        {
            int HotelID = 0;
            HttpContext.Session.Remove("HotelID");


            if (model.SelectedHotel != null)
            {
                HotelID = Convert.ToInt32(model.SelectedHotel);            
                HttpContext.Session.SetInt("HotelID", HotelID);
                //need to add Acoounts controller online users
                return RedirectToAction("Dashboard");
            }
            else
               return NotFound();
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
