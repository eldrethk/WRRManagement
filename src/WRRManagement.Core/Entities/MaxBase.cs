using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class MaxBase
    {
        public int ID { get; private set; }
        public int RoomTypeID { get; private set; }
        public int MaxBaseCount { get; private set; }
        public int BaseCount { get; private set; }

        public MaxBase() { }

        public static MaxBase Create(
            int roomTyeID,
            int maxBaseCount,
            int baseCount) 
        {
            if(roomTyeID < 0)
                throw new ArgumentException("Room Type must be a valid room" , nameof(roomTyeID));
            
            if(maxBaseCount < 0)
                throw new ArgumentException("Max Base count can not be negative", nameof(maxBaseCount));
            
            if(baseCount < 0)
                throw new ArgumentException("Base count can not be negative", nameof(baseCount));

            if (baseCount > maxBaseCount)
                throw new ArgumentException("Base count can not be greater than Max Base count", nameof(baseCount));

            return new MaxBase 
            {
                RoomTypeID = roomTyeID,
                MaxBaseCount = maxBaseCount,
                BaseCount = baseCount
            };
        }

        public void UpdateMaxBase(
           int roomTyeID,
            int maxBaseCount,
            int baseCount)
        {
            if (roomTyeID < 0)
                throw new ArgumentException("Room Type must be a valid room", nameof(roomTyeID));

            if (maxBaseCount < 0)
                throw new ArgumentException("Max Base count can not be negative", nameof(maxBaseCount));

            if (baseCount < 0)
                throw new ArgumentException("Base count can not be negative", nameof(baseCount));

            if (baseCount > maxBaseCount)
                throw new ArgumentException("Base count can not be greater than Max Base count", nameof(baseCount));

            RoomTypeID = roomTyeID;
            MaxBaseCount = maxBaseCount;
            BaseCount = baseCount;

        }
    }
}
