using System.ComponentModel.DataAnnotations;

namespace AnonymousComplaintsAPI.DTOs.Requests
{
    public class MergeComplaintsRequest
    {
        [Required]
        public int TargetId { get; set; }

        [Required, MinLength(1)]
        public List<int> SourceIds { get; set; } = new();
    }
}
