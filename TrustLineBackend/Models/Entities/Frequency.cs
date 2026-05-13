using System;
using System.Collections.Generic;

namespace AnonymousComplaintsAPI.Models.Entities;

public partial class Frequency
{
    public int FrequencyId { get; set; }

    public string Label { get; set; } = null!;

    public bool? Archived { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AnonymousComplaint> AnonymousComplaints { get; set; } = new List<AnonymousComplaint>();

    public virtual User? CreatedByNavigation { get; set; }
}
