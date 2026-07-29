using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;
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
            var amenity = await extraAmenityRepository.GetByIdAsync(id);
            var packages = await packageAmenityRepository.GetPackagesForAmenityAsync(id);
            AmenityViewModel model = new AmenityViewModel
            {
                Amenity = amenity,
                Packages = packages.ToList(),
                AmenityType = amenity.PricingType.ToString()
            };
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
                    int hotelId = HttpContext.Session.GetInt("HotelID");
                    var pricingType = Enum.Parse<AmenityPricingType>(model.AmenityType);
                    var description = HttpUtility.HtmlDecode(model.Amenity.Description);

                    string pictureUrl = string.Empty;
                    if (model.UploadImage != null)
                    {
                        var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png" };
                        if (!supportedTypes.Contains(model.UploadImage.Image.ContentType))
                        {
                            ModelState.AddModelError("Image", "Invalid type. Only JPG and PNG are allowed.");
                            return View(model);
                        }
                        var uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "img/amenity-images");
                        var fileName = Guid.NewGuid().ToString() + "_" + hotelId.ToString() + "_" + model.UploadImage.Image.FileName;
                        var path = Path.Combine(uploadFolder, fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.UploadImage.Image.CopyToAsync(fileStream);
                        }
                        pictureUrl = fileName;
                    }

                    var amenity = ExtraAmenity.Create(
                        hotelId,
                        model.Amenity.Name,
                        model.Amenity.ShortDescription,
                        description,
                        model.Amenity.AmenityRate,
                        model.Amenity.Tax,
                        pricingType,
                        model.ViewOnRackRate,
                        mandatory: false,
                        mandatoryQty: null,
                        discountRegularRate: pricingType == AmenityPricingType.Discount ? model.Amenity.DiscountRegularRate : null,
                        pictureUrl: pictureUrl,
                        additionalPurchases: model.Amenity.AdditionalPurchases);

                    await extraAmenityRepository.AddAsync(amenity);
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
            var packages = await packageAmenityRepository.GetPackagesForAmenityAsync(id);

            AmenityViewModel model = new AmenityViewModel
            {
                Amenity = extraAmenity,
                ViewRate = false,
                ViewOnRackRate = extraAmenity.ViewOnRackRate,
                AmenityType = extraAmenity.PricingType.ToString(),
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
                    var existing = await extraAmenityRepository.GetByIdAsync(model.Amenity.AmenityID);
                    var pricingType = Enum.Parse<AmenityPricingType>(model.AmenityType);
                    var description = HttpUtility.HtmlDecode(model.Amenity.Description);

                    string? pictureUrl = existing?.PictureUrl ?? string.Empty;
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
                        pictureUrl = fileName;
                    }

                    existing.Update(
                        model.Amenity.Name,
                        model.Amenity.ShortDescription,
                        description,
                        model.Amenity.AmenityRate,
                        model.Amenity.Tax,
                        pricingType,
                        model.ViewOnRackRate,
                        existing.Mandatory,
                        existing.MandatoryQty,
                        pricingType == AmenityPricingType.Discount ? model.Amenity.DiscountRegularRate : null,
                        pictureUrl,
                        model.Amenity.AdditionalPurchases);

                    await extraAmenityRepository.UpdateAsync(existing);
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
