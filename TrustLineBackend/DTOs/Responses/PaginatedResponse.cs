namespace AnonymousComplaintsAPI.DTOs.Responses
{

    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Total count of items matching the filter
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Total count of main complaints (non-fused) for pagination calculation
        /// </summary>
        public int? MainComplaintsTotal { get; set; }

        /// <summary>
        /// Current page data
        /// </summary>
        public IEnumerable<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// All data (for compatibility with frontend)
        /// </summary>
        public IEnumerable<T> AllData { get; set; } = new List<T>();

        /// <summary>
        /// Request parameters used
        /// </summary>
        public object? Params { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Items per page
        /// </summary>
        public int PerPage { get; set; }

        /// <summary>
        /// Total number of pages (calculated based on MainComplaintsTotal if available, otherwise Total)
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)(MainComplaintsTotal ?? Total) / PerPage);
    }
}
