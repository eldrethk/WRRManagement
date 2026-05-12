namespace WRRManagement.Core.Entities
{
    public class PackageAllocation
    {
        public int AllocationID { get; set; }
        public int PackageID { get; set; }
        public int RoomTypeID { get; set; }
        public DateTime AllocateDate { get; set; }
        public int Quantity { get; set; }
    }
}
