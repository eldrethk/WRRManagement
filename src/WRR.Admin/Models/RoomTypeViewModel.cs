
using WRRManagement.Core.Entities;

namespace WRR.Admin.Models
{
    public class RoomTypeViewModel
    {        
        public RoomType RoomType { get; set; }
        public RoomImage? RoomImage { get; set; }
        public AdultBase AdultBaseFee { get; set; }
        public MaxBase MaxBaseFee { get; set; }
        public string BaseFeeType { get; set; } = "Adult";
        
    }
}
