namespace AnonymousComplaintsAPI.DTOs.Responses
{
    /// <summary>
    /// Full profile data response from HrLink external API
    /// </summary>
    public class FullProfileResponseDto
    {
        public int UserID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Matricule { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? phonePro { get; set; }
        public string? Gender { get; set; }
        public bool? Archive { get; set; }
    }
}
