using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WRRManagement.Application.Reservations;
using WRRManagement.Application.Reservations.Dtos;
using WRRManagement.Core.DTOs;

namespace WRRManagement.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/reservations")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new guest reservation. Returns the new reservation ID.
        /// </summary>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateReservationDto dto, CancellationToken ct)
        {
            if (dto.ArrivalDate >= dto.DepartureDate)
                return BadRequest(ApiResponse.ErrorResponse("Arrival date must be before departure date"));

            try
            {
                var reservationId = await _reservationService.CreateAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = reservationId },
                    ApiResponse<int>.SuccessResponse(reservationId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create reservation for hotel {HotelId}", dto.HotelId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to create reservation"));
            }
        }

        /// <summary>
        /// Placeholder — returns the reservation ID to satisfy the 201 Location header.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<int>> GetById(int id) =>
            Ok(ApiResponse<int>.SuccessResponse(id));
    }
}
