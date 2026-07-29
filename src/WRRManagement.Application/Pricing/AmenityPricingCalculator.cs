using WRRManagement.Core.Entities;
using WRRManagement.Core.Enums;

namespace WRRManagement.Application.Pricing
{
    public record AmenityChargeResult(
        int AmenityId,
        string Name,
        AmenityPricingType PricingType,
        int NumPeople,
        int NumNights,
        decimal ChargeAmount,
        decimal TaxRate,
        decimal Tax,
        decimal TotalCharge,
        bool Mandatory,
        decimal? DiscountRegularRate);

    /// <summary>
    /// Computes the charge for one selected ExtraAmenity over a stay, per its AmenityPricingType.
    /// NOTE: the original legacy CalculateRates amenity formulas were not recoverable from the old
    /// codebase (only the six pricing-type names survived) — the per-type formulas below are a
    /// best-effort, documented interpretation of what each name implies, not a byte-for-byte port.
    /// Flag for review if the real numbers need to match a specific legacy output.
    /// </summary>
    public static class AmenityPricingCalculator
    {
        public static AmenityChargeResult Calculate(
            ExtraAmenity amenity,
            int numPeople,
            int numNights,
            bool mandatoryOverride,
            int? mandatoryQuantityOverride)
        {
            if (numNights <= 0)
                throw new ArgumentException("Number of nights must be positive", nameof(numNights));

            bool mandatory = mandatoryOverride;
            int people = mandatory && mandatoryQuantityOverride is > 0
                ? mandatoryQuantityOverride.Value
                : Math.Max(numPeople, 0);

            decimal charge = amenity.PricingType switch
            {
                AmenityPricingType.PerDayPerPerson => amenity.AmenityRate * numNights * Math.Max(people, 1),
                AmenityPricingType.PerDay => amenity.AmenityRate * numNights,
                AmenityPricingType.PerNightStay => amenity.AmenityRate,
                AmenityPricingType.OneTimeFee => amenity.AmenityRate,
                AmenityPricingType.OneTimeFeePerson => amenity.AmenityRate * Math.Max(people, 1),
                AmenityPricingType.Discount => amenity.AmenityRate,
                _ => throw new ArgumentOutOfRangeException(nameof(amenity), "Unknown amenity pricing type")
            };

            decimal tax = amenity.Tax > 0 ? charge * (amenity.Tax / 100) : 0;
            decimal totalCharge = charge + tax;

            return new AmenityChargeResult(
                amenity.AmenityID,
                amenity.Name,
                amenity.PricingType,
                people,
                numNights,
                charge,
                amenity.Tax,
                tax,
                totalCharge,
                mandatory,
                amenity.PricingType == AmenityPricingType.Discount ? amenity.DiscountRegularRate : null);
        }
    }
}
