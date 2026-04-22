using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WRRManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]   
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Simple ping endpoint - Returns 200 OK if the API is running.
        ///
        /// USE CASE:
        /// - Load balancer health checks
        /// - Quick "is it running" checks
        /// - Smoke tests after deployment
        ///
        /// NO AUTHENTICATION required - must be accessible to load balancers.
        /// </summary>

        [HttpGet("ping")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }
        /// <summary>
        /// Readiness check - Is the application ready to receive traffic?
        ///
        /// KUBERNETES PROBES:
        /// - Liveness: Is the container running? (use /api/health/ping)
        /// - Readiness: Is it ready for traffic? (use /api/health/ready)
        /// - Startup: Has it finished starting? (use /api/health/ready with delay)
        ///
        /// Returns 503 if not ready, 200 if ready.
        /// </summary>
        [HttpGet("ready")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
        public IActionResult Ready()
        {
            // In a real application, you might check:
            // - Database connection
            // - Required services availability
            // - Configuration loaded
            // - Caches warmed up

            // For this template, we'll just return ready
            return Ok(new
            {
                status = "ready",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Version information endpoint.
        ///
        /// Returns information about the running API version.
        /// Useful for deployment verification.
        /// </summary>
        [HttpGet("version")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult Version()
        {
            var assembly = typeof(HealthController).Assembly;
            var version = assembly.GetName().Version?.ToString() ?? "1.0.0";

            return Ok(new
            {
                apiVersion = version,
                frameworkVersion = Environment.Version.ToString(),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                machineName = Environment.MachineName,
                osVersion = Environment.OSVersion.ToString()
            });
        }

    }
}
