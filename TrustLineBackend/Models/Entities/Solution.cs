using System;
using System.Collections.Generic;

namespace AnonymousComplaintsAPI.Models.Entities;

public partial class Solution
{
    public int SolutionId { get; set; }

    public int? AnonymousComplaintId { get; set; }

    public string Content { get; set; } = null!;

    public bool? Archived { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AnonymousComplaint? AnonymousComplaint { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
