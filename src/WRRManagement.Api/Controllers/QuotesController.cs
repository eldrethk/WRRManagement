using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WRRManagement.Application.Pricing;
using WRRManagement.Application.Pricing.Dtos;
using WRRManagement.Core.DTOs;

namespace WRRManagement.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/hotels/{hotelId:int}/quotes")]
    public class QuotesController : ControllerBase
    {
        private readonly IQuoteService _quoteService;
        private readonly ILogger<QuotesController> _logger;

        public QuotesController(IQuoteService quoteService, ILogger<QuotesController> logger)
        {
            _quoteService = quoteService;
            _logger = logger;
        }

        /// <summary>
        /// Computes a full price breakdown for a room (optionally under a package) plus selected
        /// amenities. Pricing is always calculated server-side — never trust a client-submitted total.
        /// No persistence.
        /// </summary>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiResponse<QuoteResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<QuoteResponseDto>>> GetQuote(
            int hotelId, [FromBody] QuoteRequestDto request, CancellationToken ct)
        {
            if (request.HotelId != hotelId)
                return BadRequest(ApiResponse.ErrorResponse("Route hotelId does not match request body"));

            try
            {
                var quote = await _quoteService.GetQuoteAsync(request, ct);
                return Ok(ApiResponse<QuoteResponseDto>.SuccessResponse(quote));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compute quote for hotel {HotelId}", hotelId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to compute quote"));
            }
        }
    }
}
