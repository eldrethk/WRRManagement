using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WRRManagement.Core.Enums;

namespace WRRManagement.Application.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDisplayName(this PaymentType paymentType)
        {
            return paymentType switch
            {
                PaymentType.Visa => "Visa",
                PaymentType.AmericanExpress => "American Express",
                PaymentType.MasterCard => "Mastercard",
                PaymentType.Discover => "Discover",
                PaymentType.DinersClub => "Diners Club",
                _ => paymentType.ToString()
            };
        }

        public static string ToDisplayName(this RateDisplayMethod method)
        {
            return method switch
            {
                RateDisplayMethod.AveragePerDay => "Avg Per Day",
                RateDisplayMethod.Total => "Total",
                RateDisplayMethod.Subtotal => "Subtotal",
                _ => method.ToString()
            };

        }

        public static string ToDisplayName(this RateBreakdownMethod method)
        {
            return method switch
            {
                RateBreakdownMethod.DailyRates => "Daily Rates",
                RateBreakdownMethod.Basic => "Basic",
                _ => method.ToString()
            };
        }

        public static string ToDisplayName(this DepositCalculationMethod method)
        {
            return method switch
            {
                DepositCalculationMethod.FirstNightRoomStay => "First Night Room Stay",
                DepositCalculationMethod.FirstTwoNightsRoomStay => "First 2 Nights Room Stay",
                DepositCalculationMethod.PercentageOfTotal => "Percentage of Total",
                DepositCalculationMethod.TotalReservation => "Total Reservation",
                _ => method.ToString()
            };
        }

        public static string ToDisplayName(this ResortFeeCalculationMethod method)
        {
            return method switch
            {
                ResortFeeCalculationMethod.FlatFee => "Flat Fee",
                ResortFeeCalculationMethod.FlatFeePerDay => "Flat Fee Per Day",
                ResortFeeCalculationMethod.FlatFeePerPerson => "Flat Fee Per Person",
                _ => method.ToString()
            };
        }
    }
}
