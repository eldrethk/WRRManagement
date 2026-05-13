using Microsoft.AspNetCore.Mvc.Rendering;
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class RackRackViewModel
    {
        public List<RackRate>? RackRates { get; set; }

        public RackRate? SingleRate { get; set; }

        public RoomType? RoomType { get; set; }

        public List<RoomType>? HotelRooms { get; set; }

        public int SelectedRoomID { get; set; }

        public int HotelID { get; set; }
    }
}
