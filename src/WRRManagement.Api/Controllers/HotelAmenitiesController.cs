using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WRRManagement.Application.Amenities;
using WRRManagement.Application.Amenities.Dtos;
using WRRManagement.Core.DTOs;

namespace WRRManagement.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/hotels/{hotelId:int}/amenities")]
    public class HotelAmenitiesController : ControllerBase
    {
        private readonly IAmenityQueryService _amenityQueryService;
        private readonly ILogger<HotelAmenitiesController> _logger;

        public HotelAmenitiesController(IAmenityQueryService amenityQueryService, ILogger<HotelAmenitiesController> logger)
        {
            _amenityQueryService = amenityQueryService;
            _logger = logger;
        }

        /// <summary>
        /// All visible amenities offered by the hotel (add-ons, cards, extras).
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ExtraAmenityDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<ExtraAmenityDto>>>> GetAmenities(int hotelId, CancellationToken ct)
        {
            try
            {
                var amenities = await _amenityQueryService.GetAmenitiesForHotelAsync(hotelId, ct);
                return Ok(ApiResponse<IReadOnlyList<ExtraAmenityDto>>.SuccessResponse(amenities));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get amenities for hotel {HotelId}", hotelId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to retrieve amenities"));
            }
        }

        /// <summary>
        /// Returns true if the hotel offers any amenities (used by guest portal to conditionally show add-on step).
        /// </summary>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> HasAmenities(int hotelId, CancellationToken ct)
        {
            try
            {
                var hasAmenities = await _amenityQueryService.HotelHasAmenitiesAsync(hotelId, ct);
                return Ok(ApiResponse<bool>.SuccessResponse(hasAmenities));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check amenities for hotel {HotelId}", hotelId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to check amenities"));
            }
        }
    }
}
