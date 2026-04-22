using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.ValueObjects;

namespace WRRManagement.Core.Entities
{
    public class RackRate
    {
        public int RackRateID { get; internal set; }
        public int RoomTypeID { get; internal set; }
        public DateTime StartDate {  get; internal set; }
        public DateTime EndDate { get; internal set; }

        public decimal TierARate { get; internal set; }
        public decimal TierBRate { get; internal set; }
        public decimal TierCRate { get; internal set; }

        public bool Visible { get; internal set; }

        public RackRate() { }

        public static RackRate Create(
            int roomTypeID,
            DateTime startDate,
            DateTime endDate,
            decimal tierARate,
            decimal tierBRate,
            decimal tierCRate)
        {
            if(roomTypeID <= 0) 
                throw new ArgumentException("Room Type ID has to be a valid Room", nameof(roomTypeID));

            if(tierARate < 0)
                throw new ArgumentException("Tier Rate can not be negative", nameof(tierARate));

            if(tierBRate < 0) 
                throw new ArgumentException("Tier Rate can not be negative", nameof(tierBRate));

            if (tierCRate < 0) 
                throw new ArgumentException("Tier Rate can not be negative", nameof(tierCRate));

            if (startDate < DateTime.MinValue)
                throw new ArgumentException("Start Date must be a valid date", nameof(startDate));

            if (endDate < DateTime.MinValue)
                throw new ArgumentException("End date must be a valid date", nameof(endDate));

            _ = new DateRange(startDate, endDate);

            return new RackRate
            {
                TierARate = tierARate,
                TierBRate = tierBRate,
                TierCRate = tierCRate,
                RoomTypeID = roomTypeID,
                StartDate = startDate,
                EndDate = endDate

            };
        }

        public void UpdateRackRate(
            DateTime startDate,
            DateTime endDate,
            decimal tierARate,
            decimal tierBRate,
            decimal tierCRate)
        {
            if (tierARate < 0)
                throw new ArgumentException("Tier Rate can not be negative", nameof(tierARate));

            if (tierBRate < 0)
                throw new ArgumentException("Tier Rate can not be negative", nameof(tierBRate));

            if (tierCRate < 0)
                throw new ArgumentException("Tier Rate can not be negative", nameof(tierCRate));

            if (startDate < DateTime.MinValue)
                throw new ArgumentException("Start Date must be a valid date", nameof(startDate));

            if (endDate < DateTime.MinValue)
                throw new ArgumentException("End date must be a valid date", nameof(endDate));

            _ = new DateRange(startDate, endDate);

            StartDate = startDate;
            EndDate = endDate;
            TierARate = tierARate;
            TierBRate = tierBRate;
            TierCRate = tierCRate;

        }
        
    }
}
