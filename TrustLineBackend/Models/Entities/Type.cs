using System;
using System.Collections.Generic;

namespace AnonymousComplaintsAPI.Models.Entities;

public partial class Type
{
    public int TypeId { get; set; }

    public string Name { get; set; } = null!;

    public bool? Archived { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AnonymousComplaint> AnonymousComplaints { get; set; } = new List<AnonymousComplaint>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual User? CreatedByNavigation { get; set; }
}
