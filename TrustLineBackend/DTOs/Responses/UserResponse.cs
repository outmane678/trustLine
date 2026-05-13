namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Identifier { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
