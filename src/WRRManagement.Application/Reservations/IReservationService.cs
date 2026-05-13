using WRRManagement.Application.Reservations.Dtos;

namespace WRRManagement.Application.Reservations
{
    public interface IReservationService
    {
        Task<int> CreateAsync(CreateReservationDto dto, CancellationToken ct = default);
    }
}
