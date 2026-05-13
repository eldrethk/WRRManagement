using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRR.Admin.Extension;
using WRR.Admin.Models;


namespace WRR.Admin.Controllers
{
    public class ExtraAmenityController : Controller
    {
        private readonly IExtraAmenityRepository extraAmenityRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IPackageAmenityRepository packageAmenityRepository;

        public ExtraAmenityController(IExtraAmenityRepository extraAmenityRepository, IWebHostEnvironment hostingEnvironment, IPackageAmenityRepository packageAmenityRep)
        {
            this.extraAmenityRepository = extraAmenityRepository;
            _hostingEnvironment = hostingEnvironment;
            packageAmenityRepository = packageAmenityRep;
        }
        // GET: ExtraAmenityController
        public async Task<IActionResult> Index()
        {
            int hotelid = HttpContext.Session.GetInt("HotelID");
            var amenities = await extraAmenityRepository.GetAllForHotelAsync(hotelid);
            return View(amenities);
        }

        // GET: ExtraAmenityController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            string type = string.Empty;

            var amenity = await extraAmenityRepository.GetByIdAsync(id);
            var packages = await packageAmenityRepository.GetPackagesForAmenityAsync(id);
            AmenityViewModel model = new AmenityViewModel
            {
                Amenity = amenity,
                Packages = packages.ToList(),

            };
            if (model.Amenity.PerDayPerPerson)
                type = "Per Day Per Person";
            else if (model.Amenity.PerDay)
                type = "Per Day";
            else if (model.Amenity.PerNightStay)
                type = "Per Night Stay";
            else if (model.Amenity.OneTimeFee)
                type = "One Time Fee";
            else if (model.Amenity.OneTimeFeePerson)
                type = "One Time Fee Person";
            else if (model.Amenity.Discount)
                type = "Discount";

            model.AmenityType = type;
            return View(model);
        }

        // GET: ExtraAmenityController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExtraAmenityController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AmenityViewModel model)
        {
            try
            {


                if (ModelState.IsValid)
                {
                    int HotelId = HttpContext.Session.GetInt("HotelID");

                    model.Amenity.HotelID = HotelId;
                    model.Amenity.Description = HttpUtility.HtmlDecode(model.Amenity.Description);
                    model.Amenity.Mandatory = false;
                    model.Amenity.ViewOnRackRate = model.ViewOnRackRate;
                    model.Amenity.ViewRate = false;

                    if (model.AmenityType == "PerDayPerPerson")
                        model.Amenity.PerDayPerPerson = true;
                    else if (model.AmenityType == "PerDay")
                        model.Amenity.PerDay = true;
                    else if (model.AmenityType == "PerNightStay")
                        model.Amenity.PerNightStay = true;
                    else if (model.AmenityType == "OneTimeFee")
                        model.Amenity.OneTimeFee = true;
                    else if (model.AmenityType == "OneTimeFeePerson")
                        model.Amenity.OneTimeFeePerson = true;
                    else if (model.AmenityType == "Discount")
                        model.Amenity.Discount = true;


                    if (model.Amenity.Discount == false)
                        model.Amenity.DiscountRegularRate = 0;

                    //upload image and save to amenity
                    if (model.UploadImage != null)
                    {
                        var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png" };
                        if (!supportedTypes.Contains(model.UploadImage.Image.ContentType))
                        {
                            ModelState.AddModelError("Image", "Invalid type. Only JPG and PNG are allowed.");
                            return View(model);
                        }
                        var uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "img/amenity-images");
                        var fileName = Guid.NewGuid().ToString() + "_" + HotelId.ToString() + "_" + model.UploadImage.Image.FileName;
                        var path = Path.Combine(uploadFolder, fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.UploadImage.Image.CopyToAsync(fileStream);
                        }
                        model.Amenity.PictureUrl = fileName;
                    }
                    else
                    {
                        model.Amenity.PictureUrl = string.Empty;
                    }

                    await extraAmenityRepository.AddAsync(model.Amenity);
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }
            catch
            {
                return View(model);
            }
        }

        // GET: ExtraAmenityController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var extraAmenity = await extraAmenityRepository.GetByIdAsync(id);

            string type = string.Empty;
            if (extraAmenity.PerDayPerPerson)
                type = "PerDayPerPerson";
            else if (extraAmenity.PerDay)
                type = "PerDay";
            else if (extraAmenity.PerNightStay)
                type = "PerNightStay";
            else if (extraAmenity.OneTimeFee)
                type = "OneTimeFee";
            else if (extraAmenity.OneTimeFeePerson)
                type = "OneTimeFeePerson";
            else if (extraAmenity.Discount)
                type = "Discount";

            var packages = await packageAmenityRepository.GetPackagesForAmenityAsync(id);

            AmenityViewModel model = new AmenityViewModel
            {
                Amenity = extraAmenity,
                //Description = HttpUtility.HtmlDecode(extraAmenity.Description),
                ViewRate = false,
                ViewOnRackRate = extraAmenity.ViewOnRackRate,
                AmenityType = type,
                Packages = packages.ToList()
            };
            return View(model);
        }

        // POST: ExtraAmenityController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AmenityViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Amenity.Description = HttpUtility.HtmlDecode(model.Amenity.Description);
                    model.Amenity.ViewOnRackRate = model.ViewOnRackRate;
                    if (model.AmenityType == "PerDayPerPerson")
                        model.Amenity.PerDayPerPerson = true;
                    else if (model.AmenityType == "PerDay")
                        model.Amenity.PerDay = true;
                    else if (model.AmenityType == "PerNightStay")
                        model.Amenity.PerNightStay = true;
                    else if (model.AmenityType == "OneTimeFee")
                        model.Amenity.OneTimeFee = true;
                    else if (model.AmenityType == "OneTimeFeePerson")
                        model.Amenity.OneTimeFeePerson = true;
                    else if (model.AmenityType == "Discount")
                        model.Amenity.Discount = true;

                    if (model.Amenity.Discount == false)
                        model.Amenity.DiscountRegularRate = 0;

                    //upload image and save to amenity
                    if (model.UploadImage != null)
                    {
                        var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png" };
                        if (!supportedTypes.Contains(model.UploadImage.Image.ContentType))
                        {
                            ModelState.AddModelError("Image", "Invalid type. Only JPG and PNG are allowed.");
                            return View(model);
                        }
                        var uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "img/amenity-images");
                        var fileName = Guid.NewGuid().ToString() + "_" + model.Amenity.HotelID.ToString() + "_" + model.UploadImage.Image.FileName;
                        var path = Path.Combine(uploadFolder, fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.UploadImage.Image.CopyToAsync(fileStream);
                        }
                        model.Amenity.PictureUrl = fileName;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(model.Amenity.PictureUrl))
                            model.Amenity.PictureUrl = string.Empty;
                    }

                    await extraAmenityRepository.UpdateAsync(model.Amenity);
                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }
            catch
            {
                return View(model);
            }
        }

        // GET: ExtraAmenityController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            await extraAmenityRepository.RemoveAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Report()
        {
            SearchReportViewModel model = new SearchReportViewModel()
            {
                HotelID = HttpContext.Session.GetInt("HotelID")
            };

            return View(model);
        }

        public IActionResult DetailReport(SearchReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                int hotelid = HttpContext.Session.GetInt("HotelID");
                DateTime start = model.StartDate ?? DateTime.MinValue;
                DateTime end = model.EndDate ?? DateTime.MaxValue;
                if(hotelid > 0 && start > DateTime.MinValue && end > DateTime.MaxValue)
                {
                    //List<BookAmenity>
                }
            }

        return View(model);
        }
    }
}
