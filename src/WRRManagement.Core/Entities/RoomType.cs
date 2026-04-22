using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class RoomType
    {
        public int RoomTypeID { get; internal set; }
        public int HotelID { get; internal set; }
        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public bool AdultBase { get; internal set; }
        public bool MaxBase { get; internal set; }
        public string? BedType { get; internal set; }
        public RoomType() { }

        public static RoomType Create(
            int hotelID,
            string name, 
            string description,
            bool adultBase,
            bool maxBase,
            string? bedType
            )
        {
            if( hotelID <= 0 ) 
                throw new ArgumentException("Room Type has to be assigned to an valid hotel", nameof( hotelID));

            if (string.IsNullOrWhiteSpace(name)) 
                throw new ArgumentException("Room Name is required", nameof(name));

            if(string.IsNullOrEmpty(description)) 
                throw new ArgumentException("Room Description is required", nameof(description));

            if(adultBase == true && maxBase == true)
                throw new ArgumentException("Adult Base and Max Base can not both be used for based guest count", nameof(adultBase));

            return new RoomType
            {
                Name = name,
                Description = description,
                AdultBase = adultBase,
                MaxBase = maxBase,
                BedType = bedType
            };
        }

        public void UpdateRoomType(
            string name,
            string description,
            bool adultBase,
            bool maxBase,
            string? bedType)
        {
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Room Name is required", nameof(name));

            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Room Description is required", nameof(description));

            if (adultBase == true && maxBase == true)
                throw new ArgumentException("Adult Base and Max Base can not both be used for based guest count", nameof(adultBase));

            Name = name;
            Description = description;
            AdultBase = adultBase;
            MaxBase = maxBase;
            BedType = bedType;
        }
    }
}
