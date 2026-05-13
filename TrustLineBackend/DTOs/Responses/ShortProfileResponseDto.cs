namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class ShortProfileResponseDto
    {
        public int UserID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Identifier { get; set; }
        public string? Avatar { get; set; }
    }
}
