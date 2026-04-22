using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class RoomAllocation
    {
        public int AllocationID { get; internal set; }
        public int RoomTypeID { get; internal set; }      
        public DateTime AllocateDate { get; internal set; }
        public int Quantity { get; internal set; }

        public RoomAllocation() { }

        public static RoomAllocation Create(
            int roomTypeID,
            DateTime allocateDate,
            int quantity) 
        {
            if (roomTypeID < 0)
                throw new ArgumentException("Room Type must be a valid room", nameof(roomTypeID));
            
            if(allocateDate < DateTime.MinValue && allocateDate < DateTime.Now) 
                throw new ArgumentException("Allocate Date must be a valid date greater than today", nameof(allocateDate));

            if (quantity < 0) 
                throw new ArgumentException("Quantity can not be negative", nameof(quantity));
            
            return new RoomAllocation{ 
                RoomTypeID = roomTypeID,
                AllocateDate = allocateDate,
                Quantity = quantity
            };
        }

        public void UpdateRoomAllocation(             
            DateTime allocateDate,
            int quantity)
        {
            if (allocateDate < DateTime.MinValue && allocateDate < DateTime.Now)
                throw new ArgumentException("Allocate Date must be a valid date greater than today", nameof(allocateDate));

            if (quantity < 0)
                throw new ArgumentException("Quantity can not be negative", nameof(quantity));

            AllocateDate = allocateDate;
            Quantity = quantity; 
        }
    }
}
