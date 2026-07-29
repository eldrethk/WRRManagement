using WRRManagement.Core.Enums;

namespace WRRManagement.Core.Entities
{
    public class Package
    {
        public int PackageID { get; internal set; }
        public string Name { get; internal set; } = string.Empty;
        public string Description { get; internal set; } = string.Empty;
        public string ShortDescription { get; internal set; } = string.Empty;
        public int HotelID { get; internal set; }
        public bool Amenity { get; internal set; }
        public bool ArrMon { get; internal set; }
        public bool ArrTues { get; internal set; }
        public bool ArrWed { get; internal set; }
        public bool ArrThurs { get; internal set; }
        public bool ArrFri { get; internal set; }
        public bool ArrSat { get; internal set; }
        public bool ArrSun { get; internal set; }
        public int MinDays { get; internal set; }
        public int MaxDays { get; internal set; }
        public bool WeekendSurcharge { get; internal set; }
        public bool ResortFees { get; internal set; }
        public DateTime ValidFrom { get; internal set; }
        public DateTime ValidTo { get; internal set; }
        public DateTime EndDisplayDate { get; internal set; }
        public bool Visible { get; internal set; }
        public PackagePricingType PricingType { get; internal set; }
        public float? NumberOfNights { get; internal set; }
        public decimal? PercentageOff { get; internal set; }
        public decimal Deposit { get; internal set; }
        public bool ExtraPersonFee { get; internal set; }
        public bool PackageAllocation { get; internal set; }
        public bool DeletedPackage { get; internal set; }
        public string? SmImage { get; internal set; }
        public int Order { get; internal set; }
        public bool SpecialPage { get; internal set; }

        public Package() { }

        public static Package Create(
            int hotelId,
            string name,
            string description,
            string shortDescription,
            bool arrMon, bool arrTues, bool arrWed, bool arrThurs, bool arrFri, bool arrSat, bool arrSun,
            int minDays,
            int maxDays,
            bool weekendSurcharge,
            bool resortFees,
            DateTime validFrom,
            DateTime validTo,
            DateTime endDisplayDate,
            PackagePricingType pricingType,
            float? numberOfNights,
            decimal? percentageOff,
            bool extraPersonFee,
            bool packageAllocation,
            int order,
            bool specialPage,
            bool visible)
        {
            if (hotelId <= 0)
                throw new ArgumentException("Package must be assigned to a valid hotel", nameof(hotelId));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Package name is required", nameof(name));

            if (minDays < 0 || maxDays < 0)
                throw new ArgumentException("Min/Max days can not be negative", nameof(minDays));

            if (maxDays > 0 && minDays > maxDays)
                throw new ArgumentException("Min days can not be greater than max days", nameof(minDays));

            if (validTo <= validFrom)
                throw new ArgumentException("Valid To date must be after Valid From date", nameof(validTo));

            if (pricingType == PackagePricingType.NightsFree && (numberOfNights is null or <= 0))
                throw new ArgumentException("Number of free nights is required", nameof(numberOfNights));

            if (pricingType == PackagePricingType.PercentOff && percentageOff is null)
                throw new ArgumentException("Percentage off is required", nameof(percentageOff));

            return new Package
            {
                HotelID = hotelId,
                Name = name,
                Description = description,
                ShortDescription = shortDescription,
                ArrMon = arrMon,
                ArrTues = arrTues,
                ArrWed = arrWed,
                ArrThurs = arrThurs,
                ArrFri = arrFri,
                ArrSat = arrSat,
                ArrSun = arrSun,
                MinDays = minDays,
                MaxDays = maxDays,
                WeekendSurcharge = weekendSurcharge,
                ResortFees = resortFees,
                ValidFrom = validFrom,
                ValidTo = validTo,
                EndDisplayDate = endDisplayDate,
                PricingType = pricingType,
                NumberOfNights = pricingType == PackagePricingType.NightsFree ? numberOfNights : null,
                PercentageOff = pricingType == PackagePricingType.PercentOff ? percentageOff : null,
                ExtraPersonFee = extraPersonFee,
                PackageAllocation = packageAllocation,
                Order = order,
                SpecialPage = specialPage,
                Deposit = 0,
                Visible = visible
            };
        }

        public void Update(
            string name,
            string description,
            string shortDescription,
            bool arrMon, bool arrTues, bool arrWed, bool arrThurs, bool arrFri, bool arrSat, bool arrSun,
            int minDays,
            int maxDays,
            bool weekendSurcharge,
            bool resortFees,
            DateTime validFrom,
            DateTime validTo,
            DateTime endDisplayDate,
            PackagePricingType pricingType,
            float? numberOfNights,
            decimal? percentageOff,
            bool extraPersonFee,
            bool packageAllocation,
            int order,
            bool specialPage,
            bool visible)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Package name is required", nameof(name));

            if (minDays < 0 || maxDays < 0)
                throw new ArgumentException("Min/Max days can not be negative", nameof(minDays));

            if (maxDays > 0 && minDays > maxDays)
                throw new ArgumentException("Min days can not be greater than max days", nameof(minDays));

            if (validTo <= validFrom)
                throw new ArgumentException("Valid To date must be after Valid From date", nameof(validTo));

            if (pricingType == PackagePricingType.NightsFree && (numberOfNights is null or <= 0))
                throw new ArgumentException("Number of free nights is required", nameof(numberOfNights));

            if (pricingType == PackagePricingType.PercentOff && percentageOff is null)
                throw new ArgumentException("Percentage off is required", nameof(percentageOff));

            Name = name;
            Description = description;
            ShortDescription = shortDescription;
            ArrMon = arrMon;
            ArrTues = arrTues;
            ArrWed = arrWed;
            ArrThurs = arrThurs;
            ArrFri = arrFri;
            ArrSat = arrSat;
            ArrSun = arrSun;
            MinDays = minDays;
            MaxDays = maxDays;
            WeekendSurcharge = weekendSurcharge;
            ResortFees = resortFees;
            ValidFrom = validFrom;
            ValidTo = validTo;
            EndDisplayDate = endDisplayDate;
            PricingType = pricingType;
            NumberOfNights = pricingType == PackagePricingType.NightsFree ? numberOfNights : null;
            PercentageOff = pricingType == PackagePricingType.PercentOff ? percentageOff : null;
            ExtraPersonFee = extraPersonFee;
            PackageAllocation = packageAllocation;
            Order = order;
            SpecialPage = specialPage;
            Visible = visible;
        }

        public void SetImage(string imagePath) => SmImage = imagePath;

        public void SetInvisible() => Visible = false;
    }
}
