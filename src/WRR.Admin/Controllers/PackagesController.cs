using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Web;
using WRR.Admin.Extension;
using WRR.Admin.Models;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;
using WRRManagement.Core.Interfaces;

namespace WRR.Admin.Controllers
{
    public class PackagesController : Controller
    {
        private readonly IPackageRepository packageRep;
        private readonly IRoomTypeRepository roomRep;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IExtraAmenityRepository amenityRep;

        public PackagesController(IPackageRepository packageRep, IRoomTypeRepository roomRep, IWebHostEnvironment hostingEnvironment, IExtraAmenityRepository amenityRep)
        {
            this.packageRep = packageRep;
            this.roomRep = roomRep;
            _hostingEnvironment = hostingEnvironment;
            this.amenityRep = amenityRep;
        }

        private static PackagePricingType ParsePricingType(string packageType) => packageType switch
        {
            "Nights" => PackagePricingType.NightsFree,
            "Percentage" => PackagePricingType.PercentOff,
            "Rate" => PackagePricingType.PricePoint,
            _ => throw new ArgumentException("Unknown package type", nameof(packageType))
        };

        private static string PricingTypeToPackageType(PackagePricingType pricingType) => pricingType switch
        {
            PackagePricingType.NightsFree => "Nights",
            PackagePricingType.PercentOff => "Percentage",
            PackagePricingType.PricePoint => "Rate",
            _ => string.Empty
        };

        public async Task<IActionResult> Index()
        {
            int hotelid = HttpContext.Session.GetInt("HotelID");

            var packages = await packageRep.GetAllForHotelAsync(hotelid);
            return View(packages);
        }

        public async Task<IActionResult> Create()
        {
            int hotelid = HttpContext.Session.GetInt("HotelID");
            var rooms = await roomRep.GetAllForHotelAsync(hotelid);
            PackageViewModel model = new PackageViewModel();
            model.RoomTypes = rooms.ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PackageViewModel model)
        {
            try
            {
                int hotelid = HttpContext.Session.GetInt("HotelID");
                var rooms = await roomRep.GetAllForHotelAsync(hotelid);
                model.RoomTypes = rooms.ToList();
                if (ModelState.IsValid && hotelid > 0)
                {
                    var pricingType = ParsePricingType(model.PackageType);

                    var package = Package.Create(
                        hotelid,
                        model.Package.Name,
                        HttpUtility.HtmlDecode(model.Package.Description),
                        model.Package.ShortDescription,
                        model.Package.ArrMon, model.Package.ArrTues, model.Package.ArrWed, model.Package.ArrThurs,
                        model.Package.ArrFri, model.Package.ArrSat, model.Package.ArrSun,
                        model.Package.MinDays,
                        model.Package.MaxDays,
                        model.Package.WeekendSurcharge,
                        model.Package.ResortFees,
                        model.Package.ValidFrom,
                        model.Package.ValidTo,
                        model.Package.EndDisplayDate,
                        pricingType,
                        pricingType == PackagePricingType.NightsFree ? model.Package.NumberOfNights : null,
                        pricingType == PackagePricingType.PercentOff ? model.Package.PercentageOff : null,
                        model.Package.ExtraPersonFee,
                        model.Package.PackageAllocation,
                        model.Package.Order,
                        model.Package.SpecialPage,
                        model.Package.Visible);

                    var id = await packageRep.AddAsync(package);

                    if (model.UploadImage != null)
                    {
                        var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png" };
                        if (!supportedTypes.Contains(model.UploadImage.Image.ContentType))
                        {
                            ModelState.AddModelError("Image", "Invalid type. Only JPG and PNG are allowed.");
                            return View(model);
                        }
                        var uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "img/package-images");
                        var fileName = Guid.NewGuid().ToString() + "_" + hotelid.ToString() + "_" + model.UploadImage.Image.FileName;
                        var path = Path.Combine(uploadFolder, fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.UploadImage.Image.CopyToAsync(fileStream);
                        }
                        await packageRep.UpdateImageAsync(id, fileName);
                    }
                    if (id > 0 && model.SelectedRoomTypeIds != null)
                    {
                        await packageRep.SetRoomAssociationsAsync(id, model.SelectedRoomTypeIds.ToList());
                    }
                    else
                    {
                        ModelState.AddModelError("", "There was an error saving your package and selected room with the system");
                    }
                    return RedirectToAction("Index");

                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var package = await packageRep.GetByIdAsync(id);
            var roomTypes = await roomRep.GetAllForHotelAsync(package.HotelID);
            var packageRoomTypes = await packageRep.GetRoomTypesAsync(id);

            PackageViewModel model = new PackageViewModel
            {
                Package = package,
                PackageType = PricingTypeToPackageType(package.PricingType),
                RoomTypes = roomTypes.ToList(),
                SelectedRoomTypeIds = packageRoomTypes.Select(x => x.RoomTypeID).ToArray()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PackageViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existing = await packageRep.GetByIdAsync(model.Package.PackageID);
                    var pricingType = ParsePricingType(model.PackageType);

                    existing.Update(
                        model.Package.Name,
                        HttpUtility.HtmlDecode(model.Package.Description),
                        model.Package.ShortDescription,
                        model.Package.ArrMon, model.Package.ArrTues, model.Package.ArrWed, model.Package.ArrThurs,
                        model.Package.ArrFri, model.Package.ArrSat, model.Package.ArrSun,
                        model.Package.MinDays,
                        model.Package.MaxDays,
                        model.Package.WeekendSurcharge,
                        model.Package.ResortFees,
                        model.Package.ValidFrom,
                        model.Package.ValidTo,
                        model.Package.EndDisplayDate,
                        pricingType,
                        pricingType == PackagePricingType.NightsFree ? model.Package.NumberOfNights : null,
                        pricingType == PackagePricingType.PercentOff ? model.Package.PercentageOff : null,
                        model.Package.ExtraPersonFee,
                        model.Package.PackageAllocation,
                        model.Package.Order,
                        model.Package.SpecialPage,
                        model.Package.Visible);

                    if (model.UploadImage != null)
                    {
                        var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png" };
                        if (!supportedTypes.Contains(model.UploadImage.Image.ContentType))
                        {
                            ModelState.AddModelError("Image", "Invalid type. Only JPG and PNG are allowed.");
                            return View(model);
                        }
                        var uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "img/package-images");
                        var fileName = Guid.NewGuid().ToString() + "_" + existing.HotelID.ToString() + "_" + model.UploadImage.Image.FileName;
                        var path = Path.Combine(uploadFolder, fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.UploadImage.Image.CopyToAsync(fileStream);
                        }
                        existing.SetImage(fileName);
                    }

                    await packageRep.UpdateAsync(existing);

                    if (model.SelectedRoomTypeIds != null)
                        await packageRep.SetRoomAssociationsAsync(existing.PackageID, model.SelectedRoomTypeIds.ToList());
                    else
                    {
                        ModelState.AddModelError("", "There was an error saving your package and selected room with the system");
                    }

                    return RedirectToAction(nameof(Index));
                }
                return View(model);
            }

            catch
            {
                return View(model);

            }
        }

        public IActionResult Delete(int id)
        {
            packageRep.SetInvisibleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            Package package = await packageRep.GetByIdAsync(id);
            var rooms = await roomRep.GetAllForHotelAsync(package.HotelID);
            IEnumerable<ExtraAmenity> amenities = await amenityRep.GetPackageAmenitiesAsync(package.PackageID);

            PackageViewModel model = new PackageViewModel
            {
                Package = package,
                RoomTypes = rooms.ToList(),
                Amenities = amenities.ToList().FindAll(x => x.Mandatory == true)
            };
            return View(model);
        }
    }
}
