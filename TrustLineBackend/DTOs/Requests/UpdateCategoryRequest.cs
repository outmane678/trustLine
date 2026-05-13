namespace AnonymousComplaintsAPI.DTOs.Requests
{
    public class UpdateCategoryRequest
    {
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
