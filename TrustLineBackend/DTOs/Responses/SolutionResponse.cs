using System.ComponentModel.DataAnnotations;

namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class SolutionResponse
    {
        public int SolutionID { get; set; }

        [StringLength(8000, MinimumLength = 20, ErrorMessage = "Le contenu ne peut pas dépasser 8000 caractères.")]
        public string Content { get; set; }
        public int? AnonymousComplaintID { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? Archived { get; set; }
        public UserResponse? CreatedByUser { get; set; } = new UserResponse();

    }
}
