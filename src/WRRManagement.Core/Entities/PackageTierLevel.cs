namespace WRRManagement.Core.Entities
{
    public class PackageTierLevel
    {
        public int TierLevelID { get; set; }
        public DateTime TierDate { get; set; }
        public char Tier { get; set; }
        public int PackageID { get; set; }
    }
}
