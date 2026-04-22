using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WRRManagement.Api.HealthChecks
{
    public class ApiHealthCheck : IHealthCheck
    {
        private readonly ILogger _logger;

        public ApiHealthCheck(ILogger<ApiHealthCheck> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Check the health of the API.
        ///
        /// WHAT TO CHECK:
        /// - Internal state
        /// - Memory usage
        /// - Thread pool saturation
        /// - Custom business rules
        ///
        /// DON'T CHECK:
        /// - External dependencies (use separate health checks for those)
        /// - Expensive operations (health checks run frequently)
        /// </summary>
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                //Check memory usage
                var memoryInfo = GC.GetGCMemoryInfo();
                var usedMemory = GC.GetTotalMemory(false);
                var totalMemory = memoryInfo.TotalAvailableMemoryBytes;
                var memoryPercentage = (double)usedMemory / totalMemory * 100;

                //check thread pool
                ThreadPool.GetAvailableThreads(out int workerThreads, out int completetionPortThreads);
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

                var data = new Dictionary<string, object>
                {
                    { "MemoryUsedMB", usedMemory / 1024 / 1024 },
                    { "MemoryPercentage", Math.Round(memoryPercentage, 2) },
                    { "AvailableWorkerThreads", workerThreads },
                    { "MaxWorkerThreads", maxWorkerThreads }
                };

                //Degraded if memory usage is high
                if (memoryPercentage > 90)
                {
                    _logger.LogWarning("API health check: High memory usage ({Percentage}%)", memoryPercentage);
                    return Task.FromResult(HealthCheckResult.Degraded(
                      description: $"High memory usage: {memoryPercentage:F2}%",
                      data: data));

                }
                // Degraded if thread pool is nearly exhausted
                if (workerThreads < maxWorkerThreads * 0.1)
                {
                    _logger.LogWarning("API health check: Low available threads ({Available}/{Max})",
                        workerThreads, maxWorkerThreads);
                    return Task.FromResult(HealthCheckResult.Degraded(
                        description: "Thread pool nearly exhausted",
                        data: data));
                }
                return Task.FromResult(HealthCheckResult.Healthy(
                   description: "API is healthy",
                   data: data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API health check failed");
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    description: "API health check failed",
                    exception: ex));

            }
        }
    }
    public class StartupHealthCheck : IHealthCheck
    {
        private volatile bool _isReady;

        /// <summary>
        /// Mark the application as ready.
        /// Call this after all initialization is complete.
        /// </summary>
        /// 
        public void SetReady() => _isReady = true;
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (_isReady)
            {
                return Task.FromResult(HealthCheckResult.Healthy("Application is ready"));
            }
            return Task.FromResult(HealthCheckResult.Unhealthy("Application is starting up"));
        }
    }
}
