namespace WRRManagement.Core.Entities
{
    public class ReservationAmenity
    {
        public int ID { get; internal set; }
        public int ReservationID { get; internal set; }
        public int AmenityID { get; internal set; }
        public decimal ChargeAmount { get; internal set; }
        public decimal TaxIncluded { get; internal set; }
        public bool Mandatory { get; internal set; }
        public decimal TaxRate { get; internal set; }
        public int NumPeople { get; internal set; }
        public DateTime NumNights { get; internal set; }
        public decimal TotalCharge { get; internal set; }

        public ReservationAmenity() { }

        public static ReservationAmenity Create(
            int reservationId,
            int amenityId,
            decimal chargeAmount,
            decimal taxIncluded,
            bool mandatory,
            decimal taxRate,
            int numPeople,
            DateTime numNights,
            decimal totalCharge)
        {
            if (reservationId <= 0)
                throw new ArgumentException("Reservation Amenity must be assigned to a valid reservation", nameof(reservationId));

            if (amenityId <= 0)
                throw new ArgumentException("Reservation Amenity must be assigned to a valid amenity", nameof(amenityId));

            if (chargeAmount < 0)
                throw new ArgumentException("Charge amount can not be negative", nameof(chargeAmount));

            if (numPeople < 0)
                throw new ArgumentException("Number of people can not be negative", nameof(numPeople));

            return new ReservationAmenity
            {
                ReservationID = reservationId,
                AmenityID = amenityId,
                ChargeAmount = chargeAmount,
                TaxIncluded = taxIncluded,
                Mandatory = mandatory,
                TaxRate = taxRate,
                NumPeople = numPeople,
                NumNights = numNights,
                TotalCharge = totalCharge
            };
        }
    }
}
