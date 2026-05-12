using WRRManagement.Application.Amenities.Dtos;

namespace WRRManagement.Application.Amenities
{
    public interface IAmenityQueryService
    {
        Task<IReadOnlyList<ExtraAmenityDto>> GetAmenitiesForHotelAsync(int hotelId, CancellationToken ct = default);
        Task<bool> HotelHasAmenitiesAsync(int hotelId, CancellationToken ct = default);
    }
}
