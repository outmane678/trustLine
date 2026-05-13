using AnonymousComplaintsAPI.DTOs.Responses;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace AnonymousComplaintsAPI.DTOs.Requests
{
    public class CreateAnonymousComplaintRequest
    {
        public int Id { get; set; }

        [StringLength(8000, MinimumLength = 20,ErrorMessage = "La description doit contenir entre 20 et 8000 caractères.")]
        [ModelBinder(BinderType = typeof(NoTrimStringModelBinder))]
        public string Description { get; set; }
        public int? CategoryID { get; set; }
        public int? TypeID { get; set; }
        public int? FrequencyID { get; set; }
        public int? CreatedBy { get; set; }
        public string? State { get; set; }
        public DateTime? IncidentDate { get; set; }
        public List<SolutionResponse>? Solutions { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsIdentityVisible { get; set; }
        public List<IFormFile>? File { get; set; }
        public List<int>? FilesToDelete { get; set; }
    }
}
