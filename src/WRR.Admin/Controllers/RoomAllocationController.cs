using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Runtime.InteropServices;
using System.Text.Json;
using WRR.Admin.Extension;
using WRR.Admin.Models;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRR.Admin.Controllers
{
    public class RoomAllocationController : Controller
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IRoomAllocationRepository _roomAllocationRepository;
        private readonly ILogger<RoomAllocationController> _logger;

        public RoomAllocationController(IRoomTypeRepository roomTypeRepository, IRoomAllocationRepository roomAllocationRepository, ILogger<RoomAllocationController> logger)
        {
            _roomTypeRepository = roomTypeRepository;
            _roomAllocationRepository = roomAllocationRepository;
            _logger = logger;
        }
        // GET: RoomAllocationController

        public async Task<IActionResult> Index(int? roomid)
        {
            this.RestoreModelState();
            int id = roomid ?? 0;
            int HotelID = HttpContext.Session.GetInt("HotelID");
            var hotelRooms = (await _roomTypeRepository.GetAllForHotelAsync(HotelID)).ToList();
            string roomName = string.Empty;

            if (id > 0)
                roomName = hotelRooms.FirstOrDefault(x => x.RoomTypeID == id)?.Name ?? string.Empty;

            RoomEventViewModel roomEventViewModel = new RoomEventViewModel
            {
                Rooms = hotelRooms,
                StartDate = null,
                EndDate = null,
                Quantity = null,
                SelectedRoomTypeID = id,
                SelectRoomName = roomName
            };

            return View(roomEventViewModel);
        }


        // POST: RoomAllocationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RoomEventViewModel model)
        {
            this.RestoreModelState();
            int hotelID = HttpContext.Session.GetInt("HotelID");
            model.Rooms = (await _roomTypeRepository.GetAllForHotelAsync(hotelID)).ToList();
            if(model.SelectedRoomTypeID > 0)
                model.SelectRoomName = model.Rooms.FirstOrDefault(x => x.RoomTypeID == model.SelectedRoomTypeID)?.Name;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomEventViewModel model) {

            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if (model.StartDate > DateTime.MinValue && model.EndDate > DateTime.MinValue)
                {
                    try
                    {
                        await _roomAllocationRepository.AddDateRangeAsync(model.SelectedRoomTypeID, model.StartDate.Value, model.EndDate.Value, model.Quantity.Value);
                    }
                    catch
                    {
                        ModelState.AddModelError("", "There was an error saving your room allocation");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please enter a valid date");
                }

            }
            this.SerializeModelState();
            return RedirectToAction("Index", new { roomid = model.SelectedRoomTypeID });
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
