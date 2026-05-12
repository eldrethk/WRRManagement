namespace WRRManagement.Core.Entities
{
    public class Package
    {
        public int PackageID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public int HotelID { get; set; }
        public bool Amenity { get; set; }
        public bool ArrMon { get; set; }
        public bool ArrTues { get; set; }
        public bool ArrWed { get; set; }
        public bool ArrThurs { get; set; }
        public bool ArrFri { get; set; }
        public bool ArrSat { get; set; }
        public bool ArrSun { get; set; }
        public int MinDays { get; set; }
        public int MaxDays { get; set; }
        public bool WeekendSurcharge { get; set; }
        public bool ResortFees { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public DateTime EndDisplayDate { get; set; }
        public bool Visible { get; set; }
        public bool NightsFree { get; set; }
        public float? NumberOfNights { get; set; }
        public bool PercentOff { get; set; }
        public decimal? PercentageOff { get; set; }
        public bool PricePoint { get; set; }
        public decimal Deposit { get; set; }
        public bool ExtraPersonFee { get; set; }
        public bool PackageAllocation { get; set; }
        public bool DeletedPackage { get; set; }
        public string? SmImage { get; set; }
        public int Order { get; set; }
        public bool SpecialPage { get; set; }
    }
}
