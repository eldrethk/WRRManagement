using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Web;
using WRR.Admin.Extension;
using WRR.Admin.Models;
using WRRManagement.Core.Entities;
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
                model.RoomTypes =rooms.ToList();
                if (ModelState.IsValid && hotelid > 0)
                {

                    model.Package.HotelID = hotelid;
                    model.Package.Description = HttpUtility.HtmlDecode(model.Package.Description);
                    model.Package.SmImage = string.Empty;
                    model.Package.Deposit = 0;

                    if (model.PackageType == "Nights")
                    {
                        model.Package.NightsFree = true;
                        model.Package.PercentageOff = 0;
                    }
                    else if (model.PackageType == "Percentage")
                    {
                        model.Package.PercentOff = true;
                        model.Package.NumberOfNights = 0;
                    }
                    else if (model.PackageType == "Rate")
                    {
                        model.Package.PricePoint = true;
                        model.Package.NumberOfNights = 0;
                        model.Package.PercentageOff = 0;
                    }

                    var id = await packageRep.AddAsync(model.Package);

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
                        model.Package.SmImage = fileName;
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
            string packageType = string.Empty;
            if (package.NightsFree == true)
                packageType = "Nights";
            else if (package.PercentOff == true)
                packageType = "Percentage";
            else if (package.PricePoint == true)
                packageType = "Rate";

            var roomTypes = await roomRep.GetAllForHotelAsync(package.HotelID);
            var packageRoomTypes = await packageRep.GetRoomTypesAsync(id);

            PackageViewModel model = new PackageViewModel
            {
                Package = package,
                PackageType = packageType,
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
                    model.Package.Description = HttpUtility.HtmlDecode(model.Package.Description);
                    if (model.PackageType == "Nights")
                    {
                        model.Package.NightsFree = true;
                        model.Package.PercentageOff = 0;
                    }
                    else if (model.PackageType == "Percentage")
                    {
                        model.Package.PercentOff = true;
                        model.Package.NumberOfNights = 0;
                    }
                    else if (model.PackageType == "Rate")
                    {
                        model.Package.PricePoint = true;
                        model.Package.NumberOfNights = 0;
                        model.Package.PercentageOff = 0;
                    }


                    if (model.UploadImage != null)
                    {
                        var supportedTypes = new[] { "image/jpg", "image/jpeg", "image/png" };
                        if (!supportedTypes.Contains(model.UploadImage.Image.ContentType))
                        {
                            ModelState.AddModelError("Image", "Invalid type. Only JPG and PNG are allowed.");
                            return View(model);
                        }
                        var uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "img/package-images");
                        var fileName = Guid.NewGuid().ToString() + "_" + model.Package.HotelID.ToString() + "_" + model.UploadImage.Image.FileName;
                        var path = Path.Combine(uploadFolder, fileName);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await model.UploadImage.Image.CopyToAsync(fileStream);
                        }
                        model.Package.SmImage = fileName;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(model.Package.SmImage))
                        {
                            model.Package.SmImage = string.Empty;
                        }

                    }
                    await packageRep.UpdateAsync(model.Package);

                    if (model.SelectedRoomTypeIds != null)
                        await packageRep.SetRoomAssociationsAsync(model.Package.PackageID, model.SelectedRoomTypeIds.ToList());
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
