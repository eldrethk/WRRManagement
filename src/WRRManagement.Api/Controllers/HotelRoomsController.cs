using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WRRManagement.Application.Rooms;
using WRRManagement.Application.Rooms.Dtos;
using WRRManagement.Core.DTOs;

namespace WRRManagement.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    public class HotelRoomsController : ControllerBase
    {
        private readonly IRoomQueryService _roomQueryService;
        private readonly ILogger<HotelRoomsController> _logger;

        public HotelRoomsController(IRoomQueryService roomQueryService, ILogger<HotelRoomsController> logger)
        {
            _roomQueryService = roomQueryService;
            _logger = logger;
        }

        /// <summary>
        /// All bookable room types for a hotel with images, features, and max-guest count.
        /// </summary>
        [HttpGet("api/hotels/{hotelId:int}/rooms")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ViewRoomDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<ViewRoomDto>>>> GetRooms(
            int hotelId, CancellationToken ct)
        {
            try
            {
                var rooms = await _roomQueryService.GetBookableRoomsAsync(hotelId, ct);
                if (rooms.Count == 0)
                    return NotFound(ApiResponse.ErrorResponse("No rooms found for this hotel"));

                return Ok(ApiResponse<IReadOnlyList<ViewRoomDto>>.SuccessResponse(rooms));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms for hotel {HotelId}", hotelId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to retrieve rooms"));
            }
        }

        /// <summary>
        /// Check availability for a specific room type.
        /// </summary>
        [HttpGet("api/hotels/{hotelId:int}/rooms/{roomTypeId:int}/availability")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckRoomAvailability(
            int hotelId, int roomTypeId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut,
            CancellationToken ct)
        {
            try
            {
                var available = await _roomQueryService.IsRoomAvailableAsync(roomTypeId, checkIn, checkOut, ct);
                return Ok(ApiResponse<bool>.SuccessResponse(available));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Availability check failed for room {RoomTypeId}", roomTypeId);
                return StatusCode(500, ApiResponse.ErrorResponse("Availability check failed"));
            }
        }

        /// <summary>
        /// Search available rooms for a date range and guest count.
        /// Returns pricing, fees, and deposit for each available room type.
        /// </summary>
        [HttpGet("api/hotels/{hotelId:int}/availability")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AvailableRackRoomDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AvailableRackRoomDto>>>> SearchAvailability(
            int hotelId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut,
            [FromQuery] int adults,
            [FromQuery] int children,
            CancellationToken ct)
        {
            try
            {
                if (checkOut <= checkIn)
                    return BadRequest(ApiResponse.ErrorResponse("Check-out must be after check-in"));

                if (adults < 1)
                    return BadRequest(ApiResponse.ErrorResponse("At least one adult is required"));

                var results = await _roomQueryService.SearchAvailabilityAsync(
                    hotelId, checkIn, checkOut, adults, children, ct);

                if (results.Count == 0)
                    return NotFound(ApiResponse.ErrorResponse("No rooms available for the selected dates and guest count"));

                return Ok(ApiResponse<IReadOnlyList<AvailableRackRoomDto>>.SuccessResponse(results));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Availability search failed for hotel {HotelId}", hotelId);
                return StatusCode(500, ApiResponse.ErrorResponse("Availability search failed"));
            }
        }
    }
}
