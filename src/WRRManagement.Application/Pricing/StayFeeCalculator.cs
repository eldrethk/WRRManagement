using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;

namespace WRRManagement.Application.Pricing
{
    public record StayFeeResult(
        decimal SubTotal,
        decimal WeekendFee,
        decimal ExtraGuestFee,
        decimal Tax,
        decimal ResortFee,
        decimal AllExtraFees,
        decimal Total,
        decimal Deposit);

    /// <summary>
    /// Weekend surcharge / extra-guest fee / tax / resort fee / deposit math, extracted from
    /// RoomQueryService.SearchAvailabilityAsync so it can be reused by the package/amenity quote
    /// engine without duplicating the room pricing rules.
    /// </summary>
    public static class StayFeeCalculator
    {
        public static StayFeeResult Calculate(
            HotelSystem system,
            IReadOnlyList<DateTime> dates,
            IReadOnlyList<decimal> dailyRates,
            int adults,
            int children,
            AdultBase? adultBase,
            MaxBase? maxBase,
            DepositCalculationMethod depositCalculationMethod,
            decimal? depositPercentage)
        {
            if (dates.Count == 0 || dates.Count != dailyRates.Count)
                throw new ArgumentException("Dates and daily rates must be the same, non-zero length", nameof(dates));

            int days = dates.Count;
            int totalGuests = adults + children;
            decimal subTotal = dailyRates.Sum();

            // Weekend fee
            decimal weekendFee = 0;
            if (system.WeekendFee > 0)
            {
                foreach (var d in dates)
                {
                    if (d.DayOfWeek == DayOfWeek.Friday || d.DayOfWeek == DayOfWeek.Saturday)
                        weekendFee += system.WeekendFee;
                }
                if (system.AddTaxToWeekendFee)
                    weekendFee *= 1 + system.TaxRate / 100;
            }

            // Extra guest fee
            decimal extraGuestFee = 0;
            if (maxBase != null)
            {
                int over = totalGuests - maxBase.MaxBaseCount;
                if (over > 0) extraGuestFee = over * system.ExtraBaseFee * days;
            }
            else if (adultBase != null)
            {
                int adultOver = adults - adultBase.AdultBaseCount;
                int childOver = children - adultBase.ChildBaseCount;
                if (adultOver > 0) extraGuestFee += adultOver * system.ExtraAdultFee * days;
                if (childOver > 0) extraGuestFee += childOver * system.ExtraChildFee * days;
            }
            if (system.AddTaxToExtraPerson && extraGuestFee > 0)
                extraGuestFee *= 1 + system.TaxRate / 100;

            decimal taxRate = system.TaxRate / 100;
            decimal tax = subTotal * taxRate;

            // Resort fee
            decimal resortFee = system.HotelResortFeeCalAs switch
            {
                ResortFeeCalculationMethod.FlatFee => system.ResortFee,
                ResortFeeCalculationMethod.FlatFeePerPerson => system.ResortFee * totalGuests,
                _ => system.ResortFee * days // FlatFeePerDay (default)
            };
            if (system.AddTaxToResortFee)
                resortFee *= 1 + taxRate;

            decimal allExtraFees = resortFee + extraGuestFee + weekendFee;
            decimal total = subTotal + tax + allExtraFees;

            // Deposit
            decimal deposit = depositCalculationMethod switch
            {
                DepositCalculationMethod.FirstTwoNightsRoomStay =>
                    (dailyRates.Count >= 2 ? dailyRates[0] + dailyRates[1] : dailyRates[0])
                    * (system.AddTaxToDeposit ? 1 + taxRate : 1),
                DepositCalculationMethod.PercentageOfTotal => depositPercentage ?? 0,
                DepositCalculationMethod.TotalReservation => total,
                _ => dailyRates[0] * (system.AddTaxToDeposit ? 1 + taxRate : 1) // FirstNightRoomStay
            };

            return new StayFeeResult(subTotal, weekendFee, extraGuestFee, tax, resortFee, allExtraFees, total, deposit);
        }
    }
}
