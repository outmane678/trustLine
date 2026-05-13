namespace AnonymousComplaintsAPI.DTOs.Responses
{
    /// <summary>
    /// Combined response containing both anonymous complaint and full user profile data
    /// </summary>
    public class ComplaintWithUserDataResponse
    {
        public AnonymousComplaintResponse? Complaint { get; set; }
        public UserProfileDataResponse? UserData { get; set; }
    }

    /// <summary>
    /// User profile data from external HrLink API
    /// </summary>
    public class UserProfileDataResponse
    {
        public int UserID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Identifier { get; set; }
        public string? Avatar { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public bool? IsActive { get; set; }
    }
}
