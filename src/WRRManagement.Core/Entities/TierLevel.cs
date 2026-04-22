using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class TierLevel
    {
        public int TierLevelID { get; internal set; }
        public DateTime TierDate { get; internal set; }
        public char Tier {  get; internal set; }
        public int HotelID { get; internal set; }

        public TierLevel() { }


        public static TierLevel Create(
            int hotelId,
            DateTime tierDate,
            char tier)
        {
            if(hotelId <= 0) 
                throw new ArgumentOutOfRangeException("Hotel ID must be a valid number", nameof(hotelId));

            tier = char.ToUpper(tier);

            if(tier != 'A' && tier != 'B' && tier != 'C') 
                throw new ArgumentOutOfRangeException("Tier level is assigned to only A, B, or C levels", nameof(tier));

            if(tierDate < DateTime.MinValue || tierDate < DateTime.Now) 
                throw new ArgumentOutOfRangeException("You must assign Tier to a valid date greater than today", nameof(tierDate));

            return new TierLevel
            {
                HotelID = hotelId,
                TierDate = tierDate,
                Tier = tier,

            };
        }

        public void UpdateTierLevel(          
            DateTime tierDate,
            char tier)
        {
            tier = char.ToUpper(tier);

            if (tier != 'A' && tier != 'B' && tier != 'C')
                throw new ArgumentOutOfRangeException("Tier level is assigned to only A, B, or C levels", nameof(tier));

            if (tierDate < DateTime.MinValue || tierDate < DateTime.Now)
                throw new ArgumentOutOfRangeException("You must assign Tier to a valid date greater than today", nameof(tierDate));

            TierDate = tierDate;
            Tier = tier;

        }

    }
}
