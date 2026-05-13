using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WRR.Admin.Extension;
using WRR.Admin.Models;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRR.Admin.Controllers
{
    public class RoomTypeController : Controller
    {
        private readonly IRoomTypeRepository roomTypeRepository;
        private readonly IAdultBaseRepository adultBaseRepository;
        private readonly IMaxBaseRepository maxBaseRepository;
        private readonly IRoomFeaturesRepository roomFeaturesRepository;

        public RoomTypeController(IRoomTypeRepository roomTypeRepository, IAdultBaseRepository adultBaseRepository, IMaxBaseRepository maxBaseRepository, IRoomFeaturesRepository roomFeaturesRep)
        {
            this.roomTypeRepository = roomTypeRepository;
            this.adultBaseRepository = adultBaseRepository;
            this.maxBaseRepository = maxBaseRepository;
            this.roomFeaturesRepository = roomFeaturesRep;
        }
        // GET: RoomTypeController
        public async Task<IActionResult> Index()
        {
            int hotelId = HttpContext.Session.GetInt("HotelID");
            var list = await roomTypeRepository.GetAllForHotelAsync(hotelId);

            return View(list);
        }

        // GET: RoomTypeController1/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var room = await roomTypeRepository.GetByIdAsync(id);
            return View(room);
        }

        // GET: RoomTypeController1/Create
        public IActionResult Create()
        {
            var model = new RoomTypeViewModel();
            return View(model);
        }

        // POST: RoomTypeController1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomTypeViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.RoomType.HotelID = HttpContext.Session.GetInt("HotelID");
                    if (model.BaseFeeType == "Adult")
                        model.RoomType.AdultBase = true;
                    else if (model.BaseFeeType == "Max")
                        model.RoomType.MaxBase = true;

                    var id = await roomTypeRepository.AddAsync(model.RoomType);

                    if (model.RoomType.AdultBase == true)
                    {
                        model.AdultBaseFee.RoomTypeID = id;
                        await adultBaseRepository.AddAsync(model.AdultBaseFee);
                    }
                    else if (model.RoomType.MaxBase == true)
                    {
                        model.MaxBaseFee.RoomTypeID = id;
                        await maxBaseRepository.AddAsync(model.MaxBaseFee);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(model.RoomType);
            }
        }

        // GET: RoomTypeController1/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            RoomTypeViewModel model = new RoomTypeViewModel();
            try
            {
                model.RoomType = await roomTypeRepository.GetByIdAsync(id);

                if (model.RoomType.AdultBase)
                {
                    model.AdultBaseFee = await adultBaseRepository.GetByRoomIDAsync(model.RoomType.RoomTypeID);
                    model.BaseFeeType = "Adult";
                }
                else
                    model.AdultBaseFee = new AdultBase();

                if (model.RoomType.MaxBase)
                {
                    model.MaxBaseFee = await maxBaseRepository.GetByRoomID(model.RoomType.RoomTypeID);
                    model.BaseFeeType = "Max";
                }
                else
                    model.MaxBaseFee = new MaxBase();

                return View(model);
            }
            catch (Exception ex) { }
            return View();
        }

        // POST: RoomTypeController1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.BaseFeeType == "Adult")
                    {
                        model.RoomType.AdultBase = true;
                        model.AdultBaseFee.RoomTypeID = model.RoomType.RoomTypeID;
                        await adultBaseRepository.AddAsync(model.AdultBaseFee);
                    }
                    else if (model.BaseFeeType == "Max")
                    {
                        model.RoomType.MaxBase = true;
                        model.MaxBaseFee.RoomTypeID = model.RoomType.RoomTypeID;
                        await maxBaseRepository.AddAsync(model.MaxBaseFee);
                    }
                    await roomTypeRepository.UpdateAsync(model.RoomType);
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: RoomTypeController1/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            await roomTypeRepository.InvisibleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteFeatures(int id, int roomid)
        {
            roomFeaturesRepository.DeleteAsync(id);
            return RedirectToAction("RoomFeatures", new { id = roomid });
        }

        public IActionResult OpenModal(int id)
        {
            RoomFeatures roomFeatures = new RoomFeatures
            {
                RoomTypeID = id
            };
            return PartialView("_RoomFeatures", roomFeatures);
        }

        [HttpGet]
        public async Task<IActionResult> RoomFeatures(int id)
        {
            var list = await roomFeaturesRepository.GetRoomFeaturesAsync(id);
            var room = await roomTypeRepository.GetByIdAsync(id);
            ViewData["RoomTypeID"] = room.RoomTypeID;
            ViewData["RoomName"] = room.Name;
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> AddFeatures(RoomFeatures model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int id = await roomFeaturesRepository.AddAsync(model);
                }
                catch
                {
                    ModelState.AddModelError("", "There was an error saving your room feature");
                }
            }
            else
                ModelState.AddModelError("", "There was an error");

            return RedirectToAction("RoomFeatures", new { id = model.RoomTypeID });
        }


    }
}
