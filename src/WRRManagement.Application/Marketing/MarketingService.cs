using Microsoft.Extensions.Logging;
using WRRManagement.Application.Marketing.Dtos;
using WRRManagement.Core.Interfaces;

namespace WRRManagement.Application.Marketing
{
    public class MarketingService : IMarketingService
    {
        private readonly IOptInEmailsRepository _optInRepo;
        private readonly ILogger<MarketingService> _logger;

        public MarketingService(IOptInEmailsRepository optInRepo, ILogger<MarketingService> logger)
        {
            _optInRepo = optInRepo;
            _logger = logger;
        }

        public async Task<int> OptInAsync(MarketingOptInDto dto, CancellationToken ct = default)
        {
            var id = await _optInRepo.AddAsync(dto.Email, dto.HotelId, dto.FirstName, dto.LastName, dto.State);
            _logger.LogInformation("Marketing opt-in recorded for {Email} (hotel {HotelId})", dto.Email, dto.HotelId);
            return id;
        }
    }
}
