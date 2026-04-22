using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.DTOs
{
    /// EXAMPLE SUCCESS:
    /// {
    ///     "success": true,
    ///     "message": "Products retrieved successfully",
    ///     "data": { "id": 1, "name": "Widget", "price": 9.99 },
    ///     "meta": { "page": 1, "totalPages": 5 }
    /// }
    ///
    /// EXAMPLE POST ERROR: 
    /// {
    ///     "success": false,
    ///     "message": "Validation failed",
    ///     "errors": ["Name is required", "Price must be positive"]
    /// }/// 
    /// 
    /// EXAMPLE GET ERROR:
    /// {
    ///     "success": false,
    ///     "message": "Product not found",
    ///     "data" : null,
    ///     "errors": null
    /// }
    /// </summary>
    public class ApiResponse<T>
    {
        //was the request successful
        public bool Success { get; set; }

        //readble message about the result
        public string Message { get; set; } = string.Empty;

        //the actual response data (null or error)
        public T? Data { get; set; }

        //list of errors messages (empty on success)
        public List<string> Errors { get; set; } = new();

        //optional metadata (pagination, etc).
        public Dictionary<string, object>? Meta { get; set; }

        // When the response was generated (UTC).
        // Useful for debugging and caching.
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // ============================================================
        // FACTORY METHODS - Convenient ways to create responses
        // ============================================================

        // Create a success response with data.
        public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }
        // Create a success response with data and metadata.
        public static ApiResponse<T> SuccessResponse(T data, string message, Dictionary<string, object> meta)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Meta = meta
            };
        }

        // Create an error response.
        public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
        /// Create an error response from an exception.
        /// WARNING SECURITY: In production, don't expose exception details!
        /// Use this only in development.
        public static ApiResponse<T> ErrorResponse(Exception ex, bool includeDetails = false)
        {
            var errors = includeDetails
                ? new List<string> { ex.Message, ex.StackTrace ?? "" }
                : new List<string>();

            return new ApiResponse<T>
            {
                Success = false,
                Message = includeDetails ? ex.Message : "An unexpected error occurred",
                Errors = errors
            };
        }
    }

    /// Non-generic version for endpoints that don't return data.
    public class ApiResponse : ApiResponse<object>
    {
        /// <summary>
        /// Create a success response without data.
        /// </summary>
        public static ApiResponse Ok(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        /// <summary>
        /// Create an error response.
        /// </summary>
        public static new ApiResponse ErrorResponse(string message, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
