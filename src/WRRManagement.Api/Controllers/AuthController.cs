using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using System.Threading.Tasks;
using WRRManagement.Application.Services;
using WRRManagement.Core.Constants;
using WRRManagement.Core.DTOs;
using WRRManagement.Core.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WRRManagement.Api.Controllers
{
    /// <summary>
    /// AUTHENTICATION CONTROLLER
    /// 
    /// Handles user login, registration, token refresh, logout,
    /// and password management.
    ///
    /// ENDPOINTS:
    ///   POST /api/auth/login          [AllowAnonymous]  — Authenticate with email/password
    ///   POST /api/auth/register       [RequireAdmin]    — Create new staff account
    ///   POST /api/auth/refresh        [AllowAnonymous]  — Exchange refresh token for new tokens
    ///   POST /api/auth/logout         [Authorize]       — Revoke refresh token
    ///   POST /api/auth/change-password [Authorize]      — Change current user's password
    ///   GET  /api/auth/me             [Authorize]       — Get current user info
    /// </summary>
    /// 
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authServices;

        public AuthController(ILogger<AuthController> logger, IAuthService authServices)
        {
            _logger = logger;
            _authServices = authServices;
        }
       

        // GET api/<AuthController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LoginRequestDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var error = GetModelStateErrors(ModelState);
                    _logger.LogWarning("Failed login attempt for {userEmail}", request.Email);
                    return Unauthorized(ApiResponse<LoginRequestDto>.ErrorResponse("Invalid credenitals"));
                }
                var token = await _authServices.LoginAsync(request);
                return Ok(ApiResponse<TokenResponseDto>.SuccessResponse(token, "Login successfull"));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed login attempt: {dto}", request);
                return Unauthorized(ApiResponse<RegisterRequestDto>.ErrorResponse("Invalid credenitals"));
            }
        }

        /// <remarks>
        /// Only Admin can create new user accounts.
        /// Role and HotelId are passed in the request body.
        /// 
        /// Rules:
        /// - Admin role: HotelId must be null
        /// - Manager/FrontDesk role: HotelId is required
        ///
        /// Sample request:
        /// 
        ///     POST /api/auth/register
        ///     {
        ///         "username": "john.doe",
        ///         "email": "john@hotel.com",
        ///         "password": "SecurePass123",
        ///         "confirmPassword": "SecurePass123",
        ///         "displayName": "John Doe",
        ///         "role": "FrontDesk",
        ///         "hotelId": 2
        ///     }
        ///         
        [HttpPost("register")]
        [Authorize(Policy = AuthConstants.Policies.RequireAdmin)]
        [ProducesResponseType(typeof(ApiResponse<RegisterRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<RegisterRequestDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<RegisterRequestDto>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<int>>> Post([FromBody] RegisterRequestDto request, string role, int hotelid)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors(ModelState);

                    return BadRequest(ApiResponse<RegisterRequestDto>.ErrorResponse("Validation failed", errors));
                }
                var userId = await _authServices.RegisterAsync(request, role, hotelid);

                _logger.LogInformation("A user {userid} was created by user : {currentuser}", userId, GetCurrentUserId());
                return Ok(ApiResponse.Ok($"User {userId} was registered"));              

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "User registation failed: {dto} : User {currentuser}", request, GetCurrentUserId());
                return BadRequest(ApiResponse<RegisterRequestDto>.ErrorResponse($"User can not be registered"));
            }
        }
        /// <summary>
        /// Exchange a refresh token for a new access token + refresh token.
        /// </summary>
        /// <remarks>
        /// Call this when the access token expires. The old refresh token
        /// is revoked and a new one is issued (token rotation).
        ///
        /// Sample request:
        /// 
        ///     POST /api/auth/refresh
        ///     {
        ///         "refreshToken": "base64-encoded-token..."
        ///     }
        /// </remarks>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenRequestDto>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenRequestDto>),StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors(ModelState);
                    _logger.LogError("Invalid Refresh Token user : {user}", GetCurrentUserId());
                    return Unauthorized(ApiResponse.ErrorResponse("Invalid, expired or revoked token"));
                }

                var rtesult = await _authServices.RefreshTokenAsync(request);
                return Ok(ApiResponse.Ok("Refresh token"));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Invalid refresh Token: {token}", request);
                return Unauthorized(ApiResponse.ErrorResponse("Invalid refresh token"));
            }
        }
        /// <summary>
        /// Exchange a refresh token for a new access token + refresh token.
        /// </summary>
        /// <remarks>
        /// Call this when the access token expires. The old refresh token
        /// is revoked and a new one is issued (token rotation).
        ///
        /// Sample request:
        /// 
        ///     POST /api/auth/refresh
        ///     {
        ///         "refreshToken": "base64-encoded-token..."
        ///     }
        /// </remarks>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse),StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> LogOut([FromBody]RefreshTokenRequestDto request)
        {
            try
            {
                await _authServices.LogoutAsync(request.RefreshToken);
                return Ok(ApiResponse.SuccessResponse("User logged out"));

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "User logged out failed: {user}", GetCurrentUserId());
                return BadRequest(ApiResponse.ErrorResponse("User logged out failed"));
            }
        }
        /// <remarks>
        /// Requires the current password for verification.
        /// After changing, all refresh tokens are revoked (forces re-login).
        ///
        /// This endpoint is exempt from the password expiry middleware,
        /// so users with expired passwords can still change them.
        ///
        /// Sample request:
        /// 
        ///     POST /api/auth/change-password
        ///     {
        ///         "currentPassword": "OldPassword123",
        ///         "newPassword": "NewPassword456",
        ///         "confirmNewPassword": "NewPassword456"
        ///     }
        ///     </remarks>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<ChangePasswordRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ChangePasswordRequestDto>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = GetModelStateErrors(ModelState);
                    _logger.LogError($"Invalid password change for user: {User}", GetCurrentUserId());
                    return Unauthorized(ApiResponse.ErrorResponse("Invalid Password change request"));
                }
                await _authServices.ChangePasswordAsync(GetCurrentUserId(), request);
                return Ok(ApiResponse.SuccessResponse("Password changed successfully"));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Invalid password change for user: {user}", GetCurrentUserId());
                return Unauthorized(ApiResponse.ErrorResponse("Invalid password change request"));
            }
        }

        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _authServices.GetCurrentUserAsync(GetCurrentUserId());

                if (user == null)
                    return NotFound(ApiResponse.ErrorResponse("User not found"));
                return Ok(ApiResponse.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get current User failed");
                return BadRequest(ApiResponse.ErrorResponse("Get current user failed"));
            }
        }

        // ============================================================
        // HELPER — Extract user ID from JWT claims
        // ============================================================
        // The user ID is stored in the "sub" (NameIdentifier) claim
        // by TokenService.GenerateAccessToken().
        //
        // Available in any [Authorize] endpoint via the User property
        // which is populated by the JWT middleware.
        // ============================================================
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Invalid user token");

            return userId;
        }

        private List<string> GetModelStateErrors(ModelStateDictionary model)
        {
            return model.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
        }
    }
}
