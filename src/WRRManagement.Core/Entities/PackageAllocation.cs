namespace WRRManagement.Core.Entities
{
    public class PackageAllocation
    {
        public int AllocationID { get; internal set; }
        public int PackageID { get; internal set; }
        public int RoomTypeID { get; internal set; }
        public DateTime AllocateDate { get; internal set; }
        public int Quantity { get; internal set; }

        public PackageAllocation() { }

        public static PackageAllocation Create(int packageId, int roomTypeId, DateTime allocateDate, int quantity)
        {
            if (packageId <= 0)
                throw new ArgumentException("Package Allocation must be assigned to a valid package", nameof(packageId));

            if (roomTypeId <= 0)
                throw new ArgumentException("Package Allocation must be assigned to a valid room", nameof(roomTypeId));

            if (quantity < 0)
                throw new ArgumentException("Quantity can not be negative", nameof(quantity));

            return new PackageAllocation
            {
                PackageID = packageId,
                RoomTypeID = roomTypeId,
                AllocateDate = allocateDate,
                Quantity = quantity
            };
        }
    }
}
