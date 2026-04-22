using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.ValueObjects
{
    public record GuestCount
    {
        public int Adults { get; }
        public int Children { get; }
        public int TotalGuests => Adults + Children;


        public GuestCount(int adults, int children)
        {
            if (adults < 1)
                throw new ArgumentException("At least one adult is required");

            if (children < 0) 
                throw new ArgumentException("Children can not be negetive");

            if (adults + children > 10)
                throw new ArgumentException("Total Guest cannot exceed 20");

            Adults = adults;
            Children = children;
        }

        public override string ToString()
        {
            if (Children == 0)
                return $"{Adults} adults{(Adults > 1 ? "s" : "")}";

            return $"{Adults} Adult{(Adults > 1 ? "s" : "")}, {Children} Child{(Children > 1 ? "ren" : "")}";

                
        }
    }
}
