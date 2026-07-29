using WRRManagement.Application.Reservations.Dtos;

namespace WRRManagement.Application.Reservations
{
    public interface IReservationService
    {
        /// <summary>
        /// Re-validates availability and recomputes pricing server-side before persisting.
        /// If <paramref name="idempotencyKey"/> matches a prior reservation, that reservation's id
        /// is returned instead of creating a duplicate.
        /// </summary>
        Task<int> CreateAsync(CreateReservationDto dto, Guid? idempotencyKey, CancellationToken ct = default);
    }
}
