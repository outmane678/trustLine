using Xunit;
using AnonymousComplaintsAPI.Repositories.Implementations;
using AnonymousComplaintsAPI.Models.Entities;
using System.Linq;
using System.Threading.Tasks;

public class AttachmentRepositoryTests
{
    [Fact]
    public async Task CreateAsync_Should_Add_Attachment()
    {
        // Arrange
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        var attachment = new Attachment
        {
            FileName = "test.pdf",
            FilePath = "/uploads/test.pdf",
            FileType = "pdf",
            Archived = false
        };

        // Act
        var result = await repo.CreateAsync(attachment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.pdf", result.FileName);
        Assert.Single(context.Attachments);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Attachment()
    {
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        var attachment = new Attachment { FileName = "file1", FilePath = "/uploads/file1" };
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByIdAsync(attachment.AttachmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("file1", result.FileName);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All()
    {
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        context.Attachments.Add(new Attachment { FileName = "A", FilePath = "/uploads/A" });
        context.Attachments.Add(new Attachment { FileName = "B", FilePath = "/uploads/B" });
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Attachment()
    {
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        var attachment = new Attachment { FileName = "delete", FilePath = "/uploads/delete" };
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync();

        // Act
        await repo.DeleteAsync(attachment.AttachmentId);

        // Assert
        Assert.Empty(context.Attachments);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True()
    {
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        var attachment = new Attachment { FileName = "exists", FilePath = "/uploads/exists" };
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync();

        var result = await repo.ExistsAsync(attachment.AttachmentId);

        Assert.True(result);
    }

    [Fact]
    public async Task ArchiveAsync_Should_Set_Archived_True()
    {
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        var attachment = new Attachment { FileName = "archive", Archived = false, FilePath = "/uploads/archive" };
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync();

        // Act
        await repo.ArchiveAsync(attachment.AttachmentId);

        var updated = await context.Attachments.FindAsync(attachment.AttachmentId);

        // Assert
        Assert.True(updated.Archived);
    }

    [Fact]
    public async Task RestoreAsync_Should_Set_Archived_False()
    {
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        var attachment = new Attachment { FileName = "restore", Archived = true, FilePath = "/uploads/restore" };
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync();

        // Act
        await repo.RestoreAsync(attachment.AttachmentId);

        var updated = await context.Attachments.FindAsync(attachment.AttachmentId);

        Assert.False(updated.Archived);
    }

    [Fact]
    public async Task GetNonArchivedAsync_Should_Filter_Archived()
    {
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        context.Attachments.Add(new Attachment { FileName = "A", Archived = false, FilePath = "/uploads/A" });
        context.Attachments.Add(new Attachment { FileName = "B", Archived = true, FilePath = "/uploads/B" });
        await context.SaveChangesAsync();

        var result = await repo.GetNonArchivedAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task ArchiveBatchAsync_Should_Archive_Multiple()
    {
        var context = DbContextFactory.Create();
        var repo = new AttachmentRepository(context);

        var a1 = new Attachment { FileName = "A", FilePath = "/uploads/A" };
        var a2 = new Attachment { FileName = "B", FilePath = "/uploads/B" };

        context.Attachments.AddRange(a1, a2);
        await context.SaveChangesAsync();

        await repo.ArchiveBatchAsync(new[] { a1.AttachmentId, a2.AttachmentId });

        var list = context.Attachments.ToList();

        Assert.All(list, x => Assert.True(x.Archived));
    }
}