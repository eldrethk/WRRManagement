using WRRManagement.Core.Entities;
using X.PagedList;

namespace WRR.Admin.Models
{
    public class DashboardViewModel
    {
        public Hotel? Hotel { get; set; }

        public IPagedList<ReservationQue> ReservationQue { get; set; } = new StaticPagedList<ReservationQue>([], 1, 10, 0);

        public ReservationStats Reservation_Booked { get; set; } = new();
        public ReservationStats Specials_Booked { get; set; } = new();
        public AmenityStats Amenitity_Booked { get; set; } = new();

        public int TodaysArrival { get; set; }
        public int TodaysDeparture { get; set; }
        public int RoomOccuied { get; set; }
    }
}
