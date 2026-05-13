namespace AnonymousComplaintsAPI.DTOs.Requests
{
    /// <summary>
    /// Request DTO for pagination and filtering
    /// </summary>
    public class PaginationRequest
    {
        /// <summary>
        /// Search query string
        /// </summary>
        public string? Q { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PerPage { get; set; } = 10;

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Filter by archived status (null = non-archived, true = archived, false = non-archived explicitly)
        /// </summary>
        public bool? Archive { get; set; }

        public int? UserId { get; set; }

        /// <summary>
        /// Filter by Type ID
        /// </summary>
        public int? TypeId { get; set; }

        /// <summary>
        /// Filter by Category ID
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Filter by complaint state (SUBMITTED, IN PROGRESS, RESOLVED)
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Filter by date range - start date
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// Filter by date range - end date
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Calculate skip count for pagination
        /// </summary>
        public int Skip => (Page - 1) * PerPage;
    }
}
