namespace WRRManagement.Core.Entities
{
    public class PackageRate
    {
        public int RateID { get; internal set; }
        public int PackageID { get; internal set; }
        public int RoomTypeID { get; internal set; }
        public DateTime StartDate { get; internal set; }
        public DateTime EndDate { get; internal set; }
        public decimal Price { get; internal set; }
        public bool Visible { get; internal set; }

        public PackageRate() { }

        public static PackageRate Create(
            int packageId,
            int roomTypeId,
            DateTime startDate,
            DateTime endDate,
            decimal price)
        {
            if (packageId <= 0)
                throw new ArgumentException("Package Rate must be assigned to a valid package", nameof(packageId));

            if (roomTypeId <= 0)
                throw new ArgumentException("Package Rate must be assigned to a valid room", nameof(roomTypeId));

            if (price < 0)
                throw new ArgumentException("Price can not be negative", nameof(price));

            if (endDate <= startDate)
                throw new ArgumentException("End Date must be after start date", nameof(endDate));

            return new PackageRate
            {
                PackageID = packageId,
                RoomTypeID = roomTypeId,
                StartDate = startDate,
                EndDate = endDate,
                Price = price,
                Visible = true
            };
        }

        public void Update(DateTime startDate, DateTime endDate, decimal price)
        {
            if (price < 0)
                throw new ArgumentException("Price can not be negative", nameof(price));

            if (endDate <= startDate)
                throw new ArgumentException("End Date must be after start date", nameof(endDate));

            StartDate = startDate;
            EndDate = endDate;
            Price = price;
        }

        public void SetVisible(bool visible) => Visible = visible;
    }
}
