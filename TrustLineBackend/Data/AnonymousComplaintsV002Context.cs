using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Models.Entities;

namespace AnonymousComplaintsAPI.Data;
public partial class AnonymousComplaintsV002Context : DbContext
{
    public AnonymousComplaintsV002Context()
    {
    }

    public AnonymousComplaintsV002Context(DbContextOptions<AnonymousComplaintsV002Context> options)
        : base(options)
    {
    }

    public virtual DbSet<AnonymousComplaint> AnonymousComplaints { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Frequency> Frequencies { get; set; }

    public virtual DbSet<Solution> Solutions { get; set; }

    public virtual DbSet<Models.Entities.Type> Types { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=10.200.0.231,1433;Initial Catalog=AnonymousComplaintV1;User ID=gmdtest;Password=M@inti17; Integrated Security=False;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnonymousComplaint>(entity =>
        {
            entity.HasKey(e => e.AnonymousComplaintId).HasName("PK__Anonymou__54E7974A0BBB4943");

            entity.HasIndex(e => e.CategoryId, "IX_AnonymousComplaintV1s_CategoryID");

            entity.HasIndex(e => e.CreatedBy, "IX_AnonymousComplaintV1s_CreatedBy");

            entity.HasIndex(e => e.FrequencyId, "IX_AnonymousComplaintV1s_FrequencyID");

            entity.HasIndex(e => e.FusionWithId, "IX_AnonymousComplaintV1s_FusionWithID");

            entity.HasIndex(e => e.TypeId, "IX_AnonymousComplaintV1s_TypeID");

            entity.Property(e => e.AnonymousComplaintId).HasColumnName("AnonymousComplaintID");
            entity.Property(e => e.Archived).HasDefaultValueSql("((0))");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FrequencyId).HasColumnName("FrequencyID");
            entity.Property(e => e.FusionWithId).HasColumnName("FusionWithID");
            entity.Property(e => e.IncidentDate).HasColumnType("date");
            entity.Property(e => e.IsIdentityVisible)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.TypeId).HasColumnName("TypeID");

            entity.HasOne(d => d.Category).WithMany(p => p.AnonymousComplaints)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_AnonymousComplaintV1_Category");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.AnonymousComplaints)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_AnonymousComplaintV1_CreatedBy");

            entity.HasOne(d => d.Frequency).WithMany(p => p.AnonymousComplaints)
                .HasForeignKey(d => d.FrequencyId)
                .HasConstraintName("FK_AnonymousComplaintV1_Frequency");

            entity.HasOne(d => d.FusionWith).WithMany(p => p.InverseFusionWith)
                .HasForeignKey(d => d.FusionWithId)
                .HasConstraintName("FK_AnonymousComplaintV1_Fusion");

            entity.HasOne(d => d.Type).WithMany(p => p.AnonymousComplaints)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK__Anonymous__TypeI__2739D489");

            entity.HasMany(d => d.Users).WithMany(p => p.AnonymousComplaintsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "Defendant",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Defendants_User"),
                    l => l.HasOne<AnonymousComplaint>().WithMany()
                        .HasForeignKey("AnonymousComplaintId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Defendants_AnonymousComplaintV1"),
                    j =>
                    {
                        j.HasKey("AnonymousComplaintId", "UserId").HasName("PK__Defendan__859F1B80B65EB45E");
                        j.ToTable("Defendants");
                        j.HasIndex(new[] { "UserId" }, "IX_Defendants_UserID");
                        j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                    });
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK__Attachme__442C64DE2E016E74");

            entity.HasIndex(e => e.AnonymousComplaintId, "IX_Attachments_AnonymousComplaintV1ID");

            entity.HasIndex(e => e.CreatedBy, "IX_Attachments_CreatedBy");

            entity.Property(e => e.AttachmentId).HasColumnName("AttachmentID");
            entity.Property(e => e.AnonymousComplaintId).HasColumnName("AnonymousComplaintID");
            entity.Property(e => e.Archived).HasDefaultValueSql("((0))");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(100);
            entity.Property(e => e.FilePath).HasMaxLength(255);
            entity.Property(e => e.FileType).HasMaxLength(255);

            entity.HasOne(d => d.AnonymousComplaint).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.AnonymousComplaintId)
                .HasConstraintName("FK_Attachments_AnonymousComplaintV1");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Attachments_CreatedBy");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B51D3BA1B");

            entity.HasIndex(e => e.CreatedBy, "IX_Categories_CreatedBy");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Archived).HasDefaultValueSql("((0))");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Categories)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Categories_CreatedBy");

            entity.HasOne(d => d.Type).WithMany(p => p.Categories)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK_Categories_Types");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.SubCategories)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Categories_ParentCategory");
        });

        modelBuilder.Entity<Frequency>(entity =>
        {
            entity.HasKey(e => e.FrequencyId).HasName("PK__Frequenc__592474B82B67A39E");

            entity.HasIndex(e => e.CreatedBy, "IX_Frequencies_CreatedBy");

            entity.Property(e => e.FrequencyId).HasColumnName("FrequencyID");
            entity.Property(e => e.Archived).HasDefaultValueSql("((0))");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Label).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Frequencies)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Frequencies_CreatedBy");
        });

        modelBuilder.Entity<Solution>(entity =>
        {
            entity.HasKey(e => e.SolutionId).HasName("PK__Solution__6B633AF077192C53");

            entity.HasIndex(e => e.AnonymousComplaintId, "IX_Solutions_AnonymousComplaintV1ID");

            entity.HasIndex(e => e.CreatedBy, "IX_Solutions_CreatedBy");

            entity.Property(e => e.SolutionId).HasColumnName("SolutionID");
            entity.Property(e => e.AnonymousComplaintId).HasColumnName("AnonymousComplaintID");
            entity.Property(e => e.Archived).HasDefaultValueSql("((0))");
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.AnonymousComplaint).WithMany(p => p.Solutions)
                .HasForeignKey(d => d.AnonymousComplaintId)
                .HasConstraintName("FK_Solutions_AnonymousComplaintV1");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Solutions)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Solutions_CreatedBy");
        });

        modelBuilder.Entity<Models.Entities.Type>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__Types__516F03959BE84BD0");

            entity.HasIndex(e => e.CreatedBy, "IX_Types_CreatedBy");

            entity.Property(e => e.TypeId).HasColumnName("TypeID");
            entity.Property(e => e.Archived).HasDefaultValueSql("((0))");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Types)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Types_CreatedBy");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC14030537");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");

            //entity.Property(e => e.Archived)
            //    .HasDefaultValueSql("((0))");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}