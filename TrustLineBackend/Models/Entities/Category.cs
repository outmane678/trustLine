using System;
using System.Collections.Generic;

namespace AnonymousComplaintsAPI.Models.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public bool? Archived { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? TypeId { get; set; }

    public int? ParentCategoryId { get; set; }

    public virtual ICollection<AnonymousComplaint> AnonymousComplaints { get; set; } = new List<AnonymousComplaint>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public virtual Type? Type { get; set; }
}
