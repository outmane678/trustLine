using System;
using System.Collections.Generic;

namespace AnonymousComplaintsAPI.Models.Entities;

public partial class AnonymousComplaint
{
    public int AnonymousComplaintId { get; set; }

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public int? FrequencyId { get; set; }

    public int? FusionWithId { get; set; }

    public bool? Archived { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? TypeId { get; set; }

    public string? State { get; set; }

    public DateTime? IncidentDate { get; set; }

    public bool? IsIdentityVisible { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual Category? Category { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Frequency? Frequency { get; set; }

    public virtual AnonymousComplaint? FusionWith { get; set; }

    public virtual ICollection<AnonymousComplaint> InverseFusionWith { get; set; } = new List<AnonymousComplaint>();

    public virtual ICollection<Solution> Solutions { get; set; } = new List<Solution>();

    public virtual Type? Type { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
