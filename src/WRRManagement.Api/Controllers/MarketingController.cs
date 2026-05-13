using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WRRManagement.Application.Marketing;
using WRRManagement.Application.Marketing.Dtos;
using WRRManagement.Core.DTOs;

namespace WRRManagement.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/marketing")]
    public class MarketingController : ControllerBase
    {
        private readonly IMarketingService _marketingService;
        private readonly ILogger<MarketingController> _logger;

        public MarketingController(IMarketingService marketingService, ILogger<MarketingController> logger)
        {
            _marketingService = marketingService;
            _logger = logger;
        }

        /// <summary>
        /// Record a guest email marketing opt-in.
        /// </summary>
        [HttpPost("opt-in")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<int>>> OptIn([FromBody] MarketingOptInDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(ApiResponse.ErrorResponse("Email is required"));

            try
            {
                var id = await _marketingService.OptInAsync(dto, ct);
                return StatusCode(StatusCodes.Status201Created, ApiResponse<int>.SuccessResponse(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record opt-in for {Email}", dto.Email);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to record opt-in"));
            }
        }
    }
}
