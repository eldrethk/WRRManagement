using WRRManagement.Application.Marketing.Dtos;

namespace WRRManagement.Application.Marketing
{
    public interface IMarketingService
    {
        Task<int> OptInAsync(MarketingOptInDto dto, CancellationToken ct = default);
    }
}
