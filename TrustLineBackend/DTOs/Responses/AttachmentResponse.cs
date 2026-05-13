namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class AttachmentResponse
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string? FileType { get; set; }
        public int? AnonymousComplaintID { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? Archived { get; set; }
    }
}
