using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Enums;

namespace WRRManagement.Core.Entities
{
    public class HotelSystem
    {
        public int SystemID { get; internal set; }
        public int HotelID { get; internal set; }

        // Display Configuration
        public RateDisplayMethod RoomRateDisplayAs { get; internal set; }
        public RateDisplayMethod PackageRateDisplayAs { get; internal set; }
        public RateBreakdownMethod RoomRateBreakdownAs { get; internal set; }
        public RateBreakdownMethod PackageRateBreakdownAs { get; internal set; }

        // Allocation & Booking Settings
        public int LowAllocationLimit { get; internal set; }
        public int PriorBook { get; internal set; }

        // Deposit Configuration
        public DepositCalculationMethod RoomDepositCalAs { get; internal set; }
        public DepositCalculationMethod PackageDepositCalAs { get; internal set; }
        public bool AddTaxToDeposit { get; internal set; }
        public decimal? DepositRoomPercentage { get; internal set; }
        public decimal? DepositPackagePercentage { get; internal set; }

        // Extra Person Fees
        public decimal ExtraAdultFee { get; internal set; }
        public decimal ExtraChildFee { get; internal set; }
        public decimal ExtraBaseFee { get; internal set; }
        public bool AddTaxToExtraPerson { get; internal set; }

        // Weekend Fee
        public decimal WeekendFee { get; internal set; }
        public bool AddTaxToWeekendFee { get; internal set; }

        // Resort Fee
        public ResortFeeCalculationMethod HotelResortFeeCalAs { get; internal set; }
        public decimal ResortFee { get; internal set; }
        public bool AddTaxToResortFee { get; internal set; }

        // Tax Rate
        public decimal TaxRate { get; internal set; }
        public HotelSystem() { }

        public static HotelSystem Create(
           int hotelId,
           decimal taxRate,
           decimal extraAdultFee,
           decimal extraChildFee,
           decimal extraBaseFee,
           decimal weekendFee,
           decimal resortFee)
        {
            // Validate required parameters
            if (hotelId <= 0)
                throw new ArgumentException("Hotel ID must be greater than zero", nameof(hotelId));

            if (taxRate < 0 || taxRate > 100)
                throw new ArgumentException("Tax rate must be between 0 and 100%", nameof(taxRate));

            if (extraAdultFee < 0)
                throw new ArgumentException("Extra adult fee cannot be negative", nameof(extraAdultFee));

            if (extraChildFee < 0)
                throw new ArgumentException("Extra child fee cannot be negative", nameof(extraChildFee));

            if (extraBaseFee < 0)
                throw new ArgumentException("Extra base fee cannot be negative", nameof(extraBaseFee));

            if (weekendFee < 0)
                throw new ArgumentException("Weekend fee cannot be negative", nameof(weekendFee));

            if (resortFee < 0)
                throw new ArgumentException("Resort fee cannot be negative", nameof(resortFee));

            return new HotelSystem
            {
                HotelID = hotelId,
                TaxRate = taxRate,
                ExtraAdultFee = extraAdultFee,
                ExtraChildFee = extraChildFee,
                ExtraBaseFee = extraBaseFee,
                WeekendFee = weekendFee,
                ResortFee = resortFee,

                // Default display settings
                RoomRateDisplayAs = RateDisplayMethod.AveragePerDay,
                PackageRateDisplayAs = RateDisplayMethod.AveragePerDay,
                RoomRateBreakdownAs = RateBreakdownMethod.DailyRates,
                PackageRateBreakdownAs = RateBreakdownMethod.DailyRates,

                // Default allocation settings
                LowAllocationLimit = 10,
                PriorBook = 0,

                // Default deposit settings
                RoomDepositCalAs = DepositCalculationMethod.FirstNightRoomStay,
                PackageDepositCalAs = DepositCalculationMethod.FirstNightRoomStay,
                AddTaxToDeposit = false,
                DepositRoomPercentage = null,
                DepositPackagePercentage = null,

                // Default tax settings
                AddTaxToExtraPerson = false,
                AddTaxToWeekendFee = false,
                AddTaxToResortFee = false,

                // Default resort fee calculation
                HotelResortFeeCalAs = ResortFeeCalculationMethod.FlatFeePerDay
            };
        }
        public void UpdateDisplaySettings(
        RateDisplayMethod displayRoomRatesAs,
        RateDisplayMethod displayPackageRatesAs,
        RateBreakdownMethod displayRoomBreakDownAs,
        RateBreakdownMethod displayPackageBreakDownAs)
        {
            //valiation needed - enum guarantees valid values
            RoomRateDisplayAs = displayRoomRatesAs;
            PackageRateDisplayAs = displayPackageRatesAs;
            RoomRateBreakdownAs = displayRoomBreakDownAs;
            PackageRateBreakdownAs = displayPackageBreakDownAs;
        }

        public void UpdateAllocationSettings(int lowAllocationLimit, int priorBook)
        {
            if (lowAllocationLimit < 0)
                throw new ArgumentException("Low allocation limit cannot be negative", nameof(lowAllocationLimit));

            if (priorBook < 0)
                throw new ArgumentException("Prior booking days cannot be negative", nameof(priorBook));

            LowAllocationLimit = lowAllocationLimit;
            PriorBook = priorBook;
        }

        public void UpdateDepositSettings(
         DepositCalculationMethod depositRoomCalAs,
         DepositCalculationMethod depositPackageCalAs,
         bool addTaxToDeposit,
         decimal? depositRoomPercentage,
         decimal? depositPackagePercentage)
        {
            if (depositRoomPercentage.HasValue && (depositRoomPercentage.Value < 0 || depositRoomPercentage.Value > 100))
                throw new ArgumentException("Room deposit percentage must be between 0 and 100%", nameof(depositRoomPercentage));

            if (depositPackagePercentage.HasValue && (depositPackagePercentage.Value < 0 || depositPackagePercentage.Value > 100))
                throw new ArgumentException("Package deposit percentage must be between 0 and 100%", nameof(depositPackagePercentage));

            RoomDepositCalAs = depositRoomCalAs;
            PackageDepositCalAs = depositPackageCalAs;
            AddTaxToDeposit = addTaxToDeposit;
            DepositRoomPercentage = depositRoomPercentage;
            DepositPackagePercentage = depositPackagePercentage;
        }

        public void UpdateExtraPersonFees(
         decimal extraAdultFee,
         decimal extraChildFee,
         decimal extraBaseFee,
         bool addTaxToExtraPerson)
        {
            if (extraAdultFee < 0)
                throw new ArgumentException("Extra adult fee cannot be negative", nameof(extraAdultFee));

            if (extraChildFee < 0)
                throw new ArgumentException("Extra child fee cannot be negative", nameof(extraChildFee));

            if (extraBaseFee < 0)
                throw new ArgumentException("Extra base fee cannot be negative", nameof(extraBaseFee));

            ExtraAdultFee = extraAdultFee;
            ExtraChildFee = extraChildFee;
            ExtraBaseFee = extraBaseFee;
            AddTaxToExtraPerson = addTaxToExtraPerson;
        }

        public void UpdateWeekendFee(decimal weekendFee, bool addTaxToWeekendFee)
        {
            if (weekendFee < 0)
                throw new ArgumentException("Weekend fee cannot be negative", nameof(weekendFee));

            WeekendFee = weekendFee;
            AddTaxToWeekendFee = addTaxToWeekendFee;
        }

        public void UpdateResortFee(ResortFeeCalculationMethod resortFeeCalAs, decimal resortFee, bool addTaxToResortFee)
        {
            if (resortFee < 0)
                throw new ArgumentException("Resort fee cannot be negative", nameof(resortFee));

            HotelResortFeeCalAs = resortFeeCalAs;
            ResortFee = resortFee;
            AddTaxToResortFee = addTaxToResortFee;
        }

        public void UpdateTaxRate(decimal taxRate)
        {
            if (taxRate < 0 || taxRate > 100)
                throw new ArgumentException("Tax rate must be between 0 and 100%", nameof(taxRate));

            TaxRate = taxRate;
        }

    }
}
