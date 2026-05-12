using WRRManagement.Core.Entities;

namespace WRRManagement.Core.Interfaces
{
    public interface IExtraAmenityRepository
    {
        Task<ExtraAmenity?> GetByIdAsync(int amenityId);
        Task<IEnumerable<ExtraAmenity>> GetAllForHotelAsync(int hotelId);
        Task<IEnumerable<ExtraAmenity>> GetRackRateAmenitiesAsync(int hotelId);
        Task<IEnumerable<ExtraAmenity>> GetPackageAmenitiesAsync(int packageId);
        Task<int> AddAsync(ExtraAmenity amenity);
        Task UpdateAsync(ExtraAmenity amenity);
        Task RemoveAsync(int amenityId);
    }
}
