namespace WRRManagement.Core.Entities
{
    public class PackageTierLevel
    {
        public int TierLevelID { get; internal set; }
        public DateTime TierDate { get; internal set; }
        public char Tier { get; internal set; }
        public int PackageID { get; internal set; }

        public PackageTierLevel() { }

        public static PackageTierLevel Create(int packageId, DateTime tierDate, char tier)
        {
            if (packageId <= 0)
                throw new ArgumentException("Package Tier Level must be assigned to a valid package", nameof(packageId));

            tier = char.ToUpper(tier);
            if (tier != 'A' && tier != 'B' && tier != 'C')
                throw new ArgumentOutOfRangeException(nameof(tier), "Tier level is assigned to only A, B, or C levels");

            return new PackageTierLevel
            {
                PackageID = packageId,
                TierDate = tierDate,
                Tier = tier
            };
        }
    }
}
