using WRRManagement.Application.Pricing.Dtos;

namespace WRRManagement.Application.Pricing
{
    public interface IQuoteService
    {
        Task<QuoteResponseDto> GetQuoteAsync(QuoteRequestDto request, CancellationToken ct = default);
    }
}
