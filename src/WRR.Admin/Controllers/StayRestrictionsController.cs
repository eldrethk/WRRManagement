using Microsoft.AspNetCore.Mvc;
using WRR.Admin.Extension;
using WRR.Admin.Models;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRR.Admin.Controllers
{
    public class StayRestrictionsController : Controller
    {
        private readonly IMinStayRepository _minStayRepository;
        private readonly IRoomTypeRepository _roomTypeRepository;

        public StayRestrictionsController(IMinStayRepository minStayRepository, IRoomTypeRepository roomTypeReposity)
        {
            _minStayRepository = minStayRepository;
            _roomTypeRepository = roomTypeReposity;
        }
        public async Task<IActionResult> Index(int? roomid)
        {
            this.RestoreModelState();
            int id = roomid ?? 0;
            int HotelID = HttpContext.Session.GetInt("HotelID");
            var hotelRooms = (await _roomTypeRepository.GetAllForHotelAsync(HotelID)).ToList();
            string roomName = string.Empty;

            if (id > 0)
                roomName = hotelRooms.FirstOrDefault(x => x.RoomTypeID == id)?.Name ?? "Room Not Found";

            RoomEventViewModel model = new RoomEventViewModel
            {
                Rooms = hotelRooms,
                StartDate = null,
                EndDate = null,
                Quantity = null,
                SelectedRoomTypeID = id,
                SelectRoomName = roomName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RoomEventViewModel model)
        {
            this.RestoreModelState();
            int hotelID = HttpContext.Session.GetInt("HotelID");
            model.Rooms = (await _roomTypeRepository.GetAllForHotelAsync(hotelID)).ToList();
            if (model.SelectedRoomTypeID > 0)
                model.SelectRoomName = model.Rooms.FirstOrDefault(x => x.RoomTypeID == model.SelectedRoomTypeID)?.Name;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomEventViewModel model) {
            if (ModelState.IsValid) {

                ModelState.Clear();
                if (model.StartDate > DateTime.MinValue && model.EndDate > DateTime.MinValue)
                {
                    try
                    {
                        await _minStayRepository.AddDateRangeAsync(model.SelectedRoomTypeID, model.StartDate.Value, model.EndDate.Value, model.Quantity.Value);
                    }
                    catch
                    {
                        ModelState.AddModelError("", "There was an error saving your stay restriction");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please enter valid dates");
                }
            }
            this.SerializeModelState();
            return RedirectToAction("Index", new {roomid = model.SelectedRoomTypeID});
        }

        public async Task<IActionResult> OpenModal(int id)
        {
            var room = await _roomTypeRepository.GetByIdAsync(id);
            RoomEventViewModel model = new RoomEventViewModel()
            {
                SelectedRoomTypeID = id,
                SelectRoomName = room.Name ?? string.Empty
            };
            return PartialView("_daterange", model);
        }
    }
}
