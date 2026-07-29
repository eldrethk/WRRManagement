using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.Enums
{
    [Flags]  //bitwise to list days of week
    public enum DaysOfWeek
    {
        None = 0,        // 0000000
        Monday = 1,      // 0000001
        Tuesday = 2,     // 0000010
        Wednesday = 4,   // 0000100
        Thursday = 8,    // 0001000
        Friday = 16,     // 0010000
        Saturday = 32,   // 0100000
        Sunday = 64,      // 1000000
        AllDays = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday,
        Weekends = Saturday | Sunday
    }

    public enum ReservationStatus 
    {
        Pending = 0,
        Confirmed = 1,
        CheckedIn = 2,
        CheckedOut = 3,
        Cancelled = 4,
        NoShow = 5
    }

    public enum PaymentType
    {
        Visa = 1,
        AmericanExpress = 2,
        MasterCard = 3,
        Discover = 4,
        DinersClub = 5
    }

    //Users authenication status
    public enum UserAuthStatus
    {
        Pending = 0,
        Active = 1,
        LockedOut = 2, 
        Closed = 3, 
        Banned = 4
    }

    /// <summary>
    /// Enum for the Hotel System domain
    /// </summary>
    /// 
    /// /// How room and package rates are displayed on the main page
    public enum RateDisplayMethod
    {
        AveragePerDay = 1,
        Subtotal = 2,
        Total = 3
    }
   
    /// How room and package rates are broken down in detail view
    public enum RateBreakdownMethod
    {
        DailyRates = 1,
        Basic = 2
    }

    /// How deposit is calculated for reservations
    public enum DepositCalculationMethod
    {
        FirstNightRoomStay = 1,
        FirstTwoNightsRoomStay = 2,
        PercentageOfTotal = 3,
        TotalReservation = 4
    }

  
    /// How resort fee is calculated
    public enum ResortFeeCalculationMethod
    {
        FlatFee = 1,
        FlatFeePerDay = 2,
        FlatFeePerPerson = 3
    }

    /// How an ExtraAmenity's charge is calculated. Replaces the legacy
    /// PerDayPerPerson/PerDay/PerNightStay/OneTimeFee/OneTimeFeePerson/Discount bit columns.
    public enum AmenityPricingType
    {
        PerDayPerPerson = 1,
        PerDay = 2,
        PerNightStay = 3,
        OneTimeFee = 4,
        OneTimeFeePerson = 5,
        Discount = 6
    }

    /// How a Package's discount is applied on top of the underlying room rate.
    /// Replaces the legacy NightsFree/PercentOff/PricePoint bit columns.
    public enum PackagePricingType
    {
        NightsFree = 1,
        PercentOff = 2,
        PricePoint = 3
    }

}
