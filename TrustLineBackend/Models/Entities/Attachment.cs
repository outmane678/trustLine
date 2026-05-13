using System;
using System.Collections.Generic;

namespace AnonymousComplaintsAPI.Models.Entities;

public partial class Attachment
{
    public int AttachmentId { get; set; }

    public int? AnonymousComplaintId { get; set; }

    public string FilePath { get; set; } = null!;

    public string? FileType { get; set; }

    public bool? Archived { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? FileName { get; set; }

    public virtual AnonymousComplaint? AnonymousComplaint { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
