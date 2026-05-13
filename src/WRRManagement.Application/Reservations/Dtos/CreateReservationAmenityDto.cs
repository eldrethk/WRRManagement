namespace WRRManagement.Application.Reservations.Dtos
{
    public class CreateReservationAmenityDto
    {
        public int AmenityId { get; init; }
        public decimal ChargeAmount { get; init; }
        public bool TaxIncluded { get; init; }
        public bool Mandatory { get; init; }
        public decimal TaxRate { get; init; }
        public int NumPeople { get; init; }
        public DateTime NumDate { get; init; }
        public decimal TotalCharge { get; init; }
    }
}
