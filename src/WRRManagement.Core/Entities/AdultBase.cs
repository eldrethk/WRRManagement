using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class AdultBase
    {
        public int ID { get; internal set; }
        public int RoomTypeID { get; internal set; }
        public int AdultBaseCount { get; internal set; }
        public int MaxAdult { get; internal set; }
        public int ChildBaseCount { get; internal set; }
        public int MaxChild { get; internal set; }
        public int MaxRoomTotal { get; internal set; }

        public AdultBase() { }

        public static AdultBase Create(
            int roomTypeID,
            int adultBaseCount,
            int maxAdult,
            int childBaseCount,
            int maxChild,
            int maxRoomTotal) 
        {  
            if(roomTypeID <= 0)
                throw new ArgumentException("Room Type ID must be a valid room", nameof(roomTypeID));

            if(adultBaseCount <= 0)
                throw new ArgumentException("Adult base count can not be negative", nameof(adultBaseCount));
            if(maxAdult <= 0)
                throw new ArgumentException("Max Adult count can not be negative", nameof(maxAdult));

            if (childBaseCount <= 0)
                throw new ArgumentException("Child base count can not be negative", nameof(childBaseCount));
            
            if (maxChild <= 0)
                throw new ArgumentException("Max child count can not be negative", nameof(maxChild));
            
            if (maxRoomTotal <= 0)
                throw new ArgumentException("Max Room Total can not be negative", nameof(maxRoomTotal));

            if (maxRoomTotal > (maxAdult + maxChild))
                throw new ArgumentException("Max total guest count can not be more than your adult and child max count", nameof(maxRoomTotal)); 
            
            return new AdultBase
            {
                RoomTypeID = roomTypeID,
                AdultBaseCount = adultBaseCount,
                MaxAdult = maxAdult,
                ChildBaseCount = childBaseCount,
                MaxChild = maxChild,
                MaxRoomTotal = maxRoomTotal

            }; 
        }

        public void UpdateAdultBase(
            int roomTypeID,
            int adultBaseCount,
            int maxAdult,
            int childBaseCount,
            int maxChild,
            int maxRoomTotal)  
        {
            if (roomTypeID <= 0)
                throw new ArgumentException("Room Type ID must be a valid room", nameof(roomTypeID));

            if (adultBaseCount <= 0)
                throw new ArgumentException("Adult base count can not be negative", nameof(adultBaseCount));
            if (maxAdult <= 0)
                throw new ArgumentException("Max Adult count can not be negative", nameof(maxAdult));

            if (childBaseCount <= 0)
                throw new ArgumentException("Child base count can not be negative", nameof(childBaseCount));

            if (maxChild <= 0)
                throw new ArgumentException("Max child count can not be negative", nameof(maxChild));

            if (maxRoomTotal <= 0)
                throw new ArgumentException("Max Room Total can not be negative", nameof(maxRoomTotal));

            if (maxRoomTotal > (maxAdult + maxChild))
                throw new ArgumentException("Max total guest count can not be more than your adult and child max count", nameof(maxRoomTotal));

            RoomTypeID = roomTypeID;
            AdultBaseCount = adultBaseCount;
            MaxAdult = maxAdult;
            ChildBaseCount = childBaseCount;
            MaxChild = maxChild;
            MaxRoomTotal = maxRoomTotal;
        }
    }
}
