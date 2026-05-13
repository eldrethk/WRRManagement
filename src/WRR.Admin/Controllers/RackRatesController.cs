using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using WRR.Admin.Extension;
using WRR.Admin.Models;
using WRRManagement.Core.Entities;
using WRRManagement.Core.Interfaces;

namespace WRR.Admin.Controllers
{
    public class RackRatesController : Controller
    {
        private readonly IRoomTypeRepository _roomTypeRep;
        private readonly IRackRateRepository _rackRateRep;

        public RackRatesController(IRoomTypeRepository roomTypeRep, IRackRateRepository rackRateRep)
        {
            _roomTypeRep = roomTypeRep;
            _rackRateRep = rackRateRep;
        }
        public async Task<IActionResult> Index(int? roomID)
        {
            this.RestoreModelState();
            int hotelID = HttpContext.Session.GetInt("HotelID");
            RackRackViewModel model = new RackRackViewModel()
            {
                HotelRooms = await GetRoomsAsync(hotelID),
                SelectedRoomID = roomID ?? -1,
                HotelID = hotelID
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RackRackViewModel model)
        {
            int hotelID = HttpContext.Session.GetInt("HotelID");
            model.HotelRooms = await GetRoomsAsync(hotelID);
            model.HotelID = hotelID;
            return View(model);
        }

        private async Task<List<RoomType>> GetRoomsAsync(int hotelId)
        {
            var roomList = new List<RoomType>();
            var rooms = await _roomTypeRep.GetAllForHotelAsync(hotelId);
            roomList.AddRange(rooms);
            roomList.Insert(0, RoomType.CreatePlaceholder(0, "All Rooms"));
            return roomList;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DateRange(DateRangeViewModel model)
        {
            if (ModelState.IsValid)
            {
                ModelState.Clear();
                if (model.SelectedRoomTypeID > 0)
                {
                    if (!await _rackRateRep.CheckDatesAysnc(model.SelectedRoomTypeID, model.StartDate, model.EndDate))
                    {
                        await AddEmptyRackRateAsync(model.SelectedRoomTypeID, model.StartDate, model.EndDate);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Date Range overlaps with another date range");
                    }
                }
                else if(model.SelectedRoomTypeID == 0)
                {
                    int HotelID = HttpContext.Session.GetInt("HotelID");
                    var roomList = (await _roomTypeRep.GetAllForHotelAsync(HotelID)).ToList();
                    foreach(RoomType room in roomList)
                    {
                        bool valid = await _rackRateRep.CheckDatesAysnc(room.RoomTypeID, model.StartDate, model.EndDate);
                        if (!valid)
                        {
                            await AddEmptyRackRateAsync(room.RoomTypeID, model.StartDate, model.EndDate);
                        }
                        else
                        {
                            ModelState.AddModelError("", room.Name + " date range overlaps with another date range");
                        }

                    }
                }
            }
            this.SerializeModelState();
            return RedirectToAction("Index", new { roomID = model.SelectedRoomTypeID});
        }

        private async Task AddEmptyRackRateAsync(int roomID, DateTime start, DateTime end)
        {
            await _rackRateRep.AddAsync(RackRate.Create(roomID, start, end, 0, 0, 0));
        }

        public async Task<IActionResult> OpenModal(int id)
        {
            string name = string.Empty;
            if (id > 0)
            {
                var room = await _roomTypeRep.GetByIdAsync(id);
                name = room.Name;
            }
            else
                name = "All Rooms";

            DateRangeViewModel model = new DateRangeViewModel()
            {
                SelectedRoomTypeID = id,
                SelectRoomName = name
            };
            return PartialView("_daterange", model);
        }

    }
}
