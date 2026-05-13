using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRR.Admin.Extension;

namespace WRR.Admin.Controllers
{
    public class HotelController : Controller
    {
        private readonly IHotelRepository hotelRepository;
        private readonly IHotelSystemRepository hotelSystemRepository;
        private readonly IDisclaimerRepository disclaimerRepository;

        public HotelController(IHotelRepository hotelRep, IHotelSystemRepository hotelSystemRepository, IDisclaimerRepository disclaimerRepository)
        {
            this.hotelRepository = hotelRep;
            this.hotelSystemRepository = hotelSystemRepository;
            this.disclaimerRepository = disclaimerRepository;
        }
        public async Task<IActionResult> Index()
        {
            int hotelID = HttpContext.Session.GetInt("HotelID");
            var hotel = await hotelRepository.GetByIdAsync(hotelID);
            return View(hotel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Hotel hotel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await hotelRepository.UpdateAsync(hotel);
                    return RedirectToAction("Dashboard", "Home");
                }

            }
            catch
            {

            }
            return View(hotel);
        }

        public async Task<IActionResult> SystemOptions()
        {
            var roomlist = new SelectList(new[] {
                new {Value = "Avg Per Day", Text = "Avg Per Day" },
                new {Value = "Subtotal", Text="Subtotal"},
                new {Value = "Total", Text = "Total"},
            }, "Value", "Text");

            var packagelist = new SelectList(new[] {
                new {Value = "Avg Per Day", Text = "Avg Per Day" },
                new {Value = "Subtotal", Text="Subtotal"},
                new {Value = "Total", Text = "Total"},
            }, "Value", "Text");

            var roombreakdownlist = new SelectList(new[]{
                new {Value = "Daily Rates", Text= "Daily Rates"},
                new {Value = "Basic", Text="Basic"}
            }, "Value", "Text");

            var packagebreakdownlist = new SelectList(new[]{
                new {Value = "Daily Rates", Text= "Daily Rates"},
                new {Value = "Basic", Text="Basic"}
            }, "Value", "Text");

            var depositlist = new SelectList(new[]
            {
                new {Value = "First Night Room Stay", Text="First Night Room Stay"},
                new {Value="First 2 Nights Room Stay", Text="First 2 Nights Room Stay" },
                new {Value="Percentage of Total", Text="Percentage of Total" },
                new {Value="Total Reservation", Text="Total Reservation"}
            }, "Value", "Text");

            var calbylist = new SelectList(new[]
            {
                new {Value="Flat Fee", Text="Flat Fee"},
                new {Value="Flat Fee Per Day", Text="Flat Fee Per Day"},
                new {Value="Flat Fee Per Person", Text="Flat Fee Per Person"}
            }, "Value", "Text");

            ViewBag.roomlist = roomlist;
            ViewBag.packagelist = packagelist;
            ViewBag.depositlist = depositlist;
            ViewBag.calbylist = calbylist;
            ViewBag.roombreakdownlist = roombreakdownlist;
            ViewBag.packagebreakdownlist = packagebreakdownlist;

            int hotelid = HttpContext.Session.GetInt("HotelID");
            var hotel = await hotelSystemRepository.GetSystemAsync(hotelid);
            return View(hotel);
        }

        // POST: HotelController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SystemOptions(HotelSystem system)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await hotelSystemRepository.UpdateAsync(system);
                    return RedirectToAction("Dashboard", "Home");
                }
                else { return View(system); }
            }
            catch
            {
                return View(system);
            }
        }

        public async Task<IActionResult> Disclaimer()
        {

            int hotelid = HttpContext.Session.GetInt("HotelID");
            var disclaimer = await disclaimerRepository.GetEmailDisclaimerAsync(hotelid);
            return View(disclaimer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Disclaimer(Disclaimer disclaimer)
        {
            int hotelid = HttpContext.Session.GetInt("HotelID");
            try
            {
                if (ModelState.IsValid)
                {
                    disclaimerRepository.UpdateReservationDisclaimerAsync(hotelid, disclaimer.ReservationDisclaimer);
                    disclaimerRepository.UpdateEmailDisclaimerAsync(hotelid, disclaimer.EmailDisclaimer);
                    return RedirectToAction("Dashboard", "Home");
                }
                else { return View(disclaimer); }
            }
            catch { return View(disclaimer); }
        }

    }
}
