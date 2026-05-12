using WRRManagement.Application.Hotels.Dtos;

namespace WRRManagement.Application.Hotels
{
    public interface IHotelQueryService
    {
        Task<HotelInfoDto?> GetHotelAsync(int hotelId, CancellationToken ct = default);
        Task<DisclaimerDto?> GetDisclaimerAsync(int hotelId, CancellationToken ct = default);
    }
}
