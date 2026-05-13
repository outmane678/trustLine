using System;
using System.Collections.Generic;

namespace AnonymousComplaintsAPI.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public bool Archive { get; set; }

    public virtual ICollection<AnonymousComplaint> AnonymousComplaints { get; set; } = new List<AnonymousComplaint>();

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Frequency> Frequencies { get; set; } = new List<Frequency>();

    public virtual ICollection<Solution> Solutions { get; set; } = new List<Solution>();

    public virtual ICollection<Type> Types { get; set; } = new List<Type>();

    public virtual ICollection<AnonymousComplaint> AnonymousComplaintsNavigation { get; set; } = new List<AnonymousComplaint>();
}
