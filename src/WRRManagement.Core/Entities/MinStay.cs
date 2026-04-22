using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class MinStay
    {
        public int MinStayID { get; internal set; }
        public int RoomTypeID { get; internal set; }
        public DateTime StayDate {  get; internal set; }
        public int Quantity {  get; internal set; }

        public MinStay() { }

        public static MinStay Create(
            int roomTypeID,
            DateTime stayDate,
            int qty)
        {
            if (roomTypeID < 0)
                throw new ArgumentException("Room Type must be a valid room", nameof(roomTypeID));

            if(stayDate < DateTime.MinValue && stayDate > DateTime.Today) 
                throw new ArgumentException("Min Stay Date must be a valid date greater than today", nameof(stayDate));

            if (qty < 0)
                throw new ArgumentException("Min night stay can not be negative", nameof(qty));

            return new MinStay 
            {
                RoomTypeID = roomTypeID,
                StayDate = stayDate,
                Quantity = qty
            };

        }

        public void UpdateMinStay(           
            DateTime stayDate,
            int qty)
        {
            if (stayDate < DateTime.MinValue && stayDate > DateTime.Today)
                throw new ArgumentException("Min Stay Date must be a valid date greater than today", nameof(stayDate));

            if (qty < 0)
                throw new ArgumentException("Min night stay can not be negative", nameof(qty));
            StayDate = stayDate;
            Quantity = qty;
        }
    }
}
