using Microsoft.AspNetCore.Mvc;
using WRR.Admin.Extension;
using WRR.Admin.Models;
using WRRManagement.Core.Interfaces;


namespace WRR.Admin.Controllers
{
    public class PackageAllocationController : Controller
    {
        private readonly IPackageAllocationRepository _packageAllocationRep;
        private readonly IPackageRepository _packageRep;

        public PackageAllocationController(IPackageAllocationRepository packageAllocationRep, IPackageRepository packageRep)
        {
            _packageAllocationRep = packageAllocationRep;
            _packageRep = packageRep;
        }
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
