using Microsoft.AspNetCore.Mvc;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;
using WRR.Admin.Extension;
using WRR.Admin.Models;


namespace WRR.Admin.Controllers
{
    public class TierLevelController : Controller
    {
        private readonly ITierLevelRepository _tierLevelRepositoty;
        private readonly IPackageTierLevelRepository _packageTierLevelRepository;
        private readonly IPackageRepository _packageRepository;

        public TierLevelController(ITierLevelRepository tierLevelRepositoty, IPackageTierLevelRepository packageTierLevelRepository, IPackageRepository packageRepository)
        {
            _tierLevelRepositoty = tierLevelRepositoty;
            _packageTierLevelRepository = packageTierLevelRepository;
            _packageRepository = packageRepository;
        }
        public IActionResult Index()
        {
            this.RestoreModelState();
            int HotelID = HttpContext.Session.GetInt("HotelID");
            TierLevelViewModel model = new TierLevelViewModel()
            {
                SelectedID = HotelID
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TierLevelViewModel model)
        {
            int HotelID = HttpContext.Session.GetInt("HotelID");
            if (ModelState.IsValid) {
                if(model.StartDate > DateTime.MinValue && model.EndDate > DateTime.MinValue)
                {

                    try
                    {
                        await _tierLevelRepositoty.AddDateRangeAsync(HotelID, model.StartDate.Value, model.EndDate.Value, model.TierLevel);
                    }
                    catch
                    {
                        ModelState.AddModelError("", "There was an error saving your tier level");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please enter valid dates");
                }
            }
            this.SerializeModelState();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PackageTier(int? packageid)
        {
            int id = packageid ?? 0;
            int HotelID = HttpContext.Session.GetInt("HotelID");
            var packages = (await _packageTierLevelRepository.GetPackagesWithTierAsync(HotelID)).ToList();

            TierLevelViewModel model = new TierLevelViewModel()
            {
                Packages = packages,
                SelectedID = id,
                StartDate = null,
                EndDate = null,
                TierLevel = 'A'
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PackageTier(TierLevelViewModel model)
        {
            int hotelID = HttpContext.Session.GetInt("HotelID");
            model.Packages = (await _packageTierLevelRepository.GetPackagesWithTierAsync(hotelID)).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PackageCreate(TierLevelViewModel model)
        {

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if (model.StartDate > DateTime.MinValue && model.EndDate > DateTime.MinValue)
                {
                    try
                    {
                        await _packageTierLevelRepository.AddDateRangeAsync(model.SelectedID, model.StartDate.Value, model.EndDate.Value, model.TierLevel);
                    }
                    catch
                    {
                        ModelState.AddModelError("", "There was an error saving your Tier level");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please enter a valid date");
                }

            }
            this.SerializeModelState();
            return RedirectToAction("PackageTier", new { packageid = model.SelectedID });
        }

    }
}
