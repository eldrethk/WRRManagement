using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Entities
{
    public class HotelUser
    {
        public int ID { get; set; }
        public int HotelID { get; set; }
        public string UserID { get; private set; } = default!;
    }
}
