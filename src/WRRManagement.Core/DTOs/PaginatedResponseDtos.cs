using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRRManagement.Core.DTOs
{
    /// PAGINATION BEST PRACTICES:
    /// - Use page-based (page/pageSize) OR cursor-based (after/limit)
    /// - Include total count for UI pagination controls
    /// - Set reasonable defaults and limits (e.g., max 100 per page)
    /// </summary>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// The items for the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Is there a previous page?
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Is there a next page?
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        /// <summary>
        /// Create a paginated response.
        /// </summary>
        public static PaginatedResponse<T> Create(List<T> items, int page, int pageSize, int totalCount)
        {
            return new PaginatedResponse<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
