using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WRRManagement.Application.Hotels;
using WRRManagement.Application.Hotels.Dtos;
using WRRManagement.Core.DTOs;

namespace WRRManagement.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/hotels/{hotelId:int}")]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelQueryService _hotelQueryService;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(IHotelQueryService hotelQueryService, ILogger<HotelsController> logger)
        {
            _hotelQueryService = hotelQueryService;
            _logger = logger;
        }

        /// <summary>
        /// Hotel info for the landing page — name, address, contact, check-in/out times.
        /// </summary>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiResponse<HotelInfoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<HotelInfoDto>>> Get(int hotelId, CancellationToken ct)
        {
            try
            {
                var hotel = await _hotelQueryService.GetHotelAsync(hotelId, ct);
                if (hotel == null)
                    return NotFound(ApiResponse.ErrorResponse("Hotel not found"));

                return Ok(ApiResponse<HotelInfoDto>.SuccessResponse(hotel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get hotel {HotelId}", hotelId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to retrieve hotel"));
            }
        }

        /// <summary>
        /// Reservation and email disclaimer text for the hotel.
        /// </summary>
        [HttpGet("disclaimer")]
        [ProducesResponseType(typeof(ApiResponse<DisclaimerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DisclaimerDto>>> GetDisclaimer(int hotelId, CancellationToken ct)
        {
            try
            {
                var disclaimer = await _hotelQueryService.GetDisclaimerAsync(hotelId, ct);
                if (disclaimer == null)
                    return NotFound(ApiResponse.ErrorResponse("Disclaimer not found"));

                return Ok(ApiResponse<DisclaimerDto>.SuccessResponse(disclaimer));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get disclaimer for hotel {HotelId}", hotelId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to retrieve disclaimer"));
            }
        }
    }
}
