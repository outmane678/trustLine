namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class ComplaintsByUserResponse
    {
        public UserProfileDataResponse? User { get; set; }
        public List<AnonymousComplaintByUserItemResponse> Data { get; set; } = new();
    }

    public class AnonymousComplaintByUserItemResponse
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int? CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public int? TypeID { get; set; }
        public string? TypeName { get; set; }
        public int? FrequencyID { get; set; }
        public string? State { get; set; }
        public bool? Archived { get; set; }
        public DateTime? IncidentDate { get; set; }
        public int? FusionWithId { get; set; }
        public List<SolutionResponse>? Solutions { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public bool? IsIdentityVisible { get; set; }
        public List<AttachmentResponse>? Attachments { get; set; }
    }
}