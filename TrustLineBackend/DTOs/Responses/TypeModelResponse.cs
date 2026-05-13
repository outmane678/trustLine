using System;
using System.ComponentModel.DataAnnotations;

namespace AnonymousComplaintsAPI.DTOs.Responses
{
    public class TypeModelResponse
    {
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères")]
        public string Name { get; set; }
        public bool? Archived { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public List<CategoryResponse>? Categories { get; set; }
    }
}
