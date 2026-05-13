using System.Runtime.InteropServices;

namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? Archived { get; set; }
        public int? TypeId { get; set; }
        public TypeModelResponse? Type { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
    }
}
