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
        private const string IdempotencyKeyHeader = "Idempotency-Key";

        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationsController> _logger;

        public ReservationsController(IReservationService reservationService, ILogger<ReservationsController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new guest reservation. Re-validates availability and recomputes pricing
        /// server-side — never trusts a client-submitted total. Pass a client-generated GUID in the
        /// Idempotency-Key header to make retries/double-clicks safe: a repeated key returns the
        /// original reservation instead of creating a duplicate.
        /// </summary>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<int>>> Create([FromBody] CreateReservationDto dto, CancellationToken ct)
        {
            Guid? idempotencyKey = null;
            if (Request.Headers.TryGetValue(IdempotencyKeyHeader, out var headerValue))
            {
                if (!Guid.TryParse(headerValue, out var parsedKey))
                    return BadRequest(ApiResponse.ErrorResponse($"{IdempotencyKeyHeader} header must be a valid GUID"));
                idempotencyKey = parsedKey;
            }

            try
            {
                var reservationId = await _reservationService.CreateAsync(dto, idempotencyKey, ct);
                return CreatedAtAction(nameof(GetById), new { id = reservationId },
                    ApiResponse<int>.SuccessResponse(reservationId));
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
