namespace WRRManagement.Core.Entities
{
    public class PackageRoomType
    {
        public int PackageRoomTypeID { get; internal set; }
        public int PackageID { get; internal set; }
        public int RoomTypeID { get; internal set; }

        public PackageRoomType() { }

        public static PackageRoomType Create(int packageId, int roomTypeId)
        {
            if (packageId <= 0)
                throw new ArgumentException("Package Room Type must be assigned to a valid package", nameof(packageId));

            if (roomTypeId <= 0)
                throw new ArgumentException("Package Room Type must be assigned to a valid room", nameof(roomTypeId));

            return new PackageRoomType
            {
                PackageID = packageId,
                RoomTypeID = roomTypeId
            };
        }
    }
}
