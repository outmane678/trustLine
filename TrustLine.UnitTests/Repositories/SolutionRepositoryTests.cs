using Xunit;
using AnonymousComplaintsAPI.Repositories.Implementations;
using AnonymousComplaintsAPI.Models.Entities;
using System.Linq;
using System.Threading.Tasks;

public class SolutionRepositoryTests
{
    [Fact]
    public async Task CreateAsync_Should_Add_Solution()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        var solution = new Solution
        {
            Content = "Test solution",
            AnonymousComplaintId = 1,
            Archived = false
        };

        var result = await repo.CreateAsync(solution);

        Assert.NotNull(result);
        Assert.Equal("Test solution", result.Content);
        Assert.Single(context.Solutions);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Solution()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        var solution = new Solution { Content = "S1" };
        context.Solutions.Add(solution);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(solution.SolutionId);

        Assert.NotNull(result);
        Assert.Equal("S1", result.Content);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        context.Solutions.Add(new Solution { Content = "A" });
        context.Solutions.Add(new Solution { Content = "B" });
        await context.SaveChangesAsync();

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_Should_Modify_Solution()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        var solution = new Solution { Content = "Old" };
        context.Solutions.Add(solution);
        await context.SaveChangesAsync();

        solution.Content = "New";

        await repo.UpdateAsync(solution);

        var updated = await context.Solutions.FindAsync(solution.SolutionId);

        Assert.Equal("New", updated.Content);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Solution()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        var solution = new Solution { Content = "ToDelete" };
        context.Solutions.Add(solution);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(solution.SolutionId);

        Assert.Empty(context.Solutions);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        var solution = new Solution { Content = "Test" };
        context.Solutions.Add(solution);
        await context.SaveChangesAsync();

        var result = await repo.ExistsAsync(solution.SolutionId);

        Assert.True(result);
    }

    [Fact]
    public async Task GetByComplaintIdAsync_Should_Filter_By_Complaint()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        context.Solutions.Add(new Solution { Content = "A", AnonymousComplaintId = 1 });
        context.Solutions.Add(new Solution { Content = "B", AnonymousComplaintId = 2 });
        await context.SaveChangesAsync();

        var result = await repo.GetByComplaintIdAsync(1);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetNonArchivedAsync_Should_Filter_Archived()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        context.Solutions.Add(new Solution { Content = "A", Archived = false });
        context.Solutions.Add(new Solution { Content = "B", Archived = true });
        await context.SaveChangesAsync();

        var result = await repo.GetNonArchivedAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetNonArchivedByComplaintIdAsync_Should_Filter_Both()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        context.Solutions.Add(new Solution { Content = "A", AnonymousComplaintId = 1, Archived = false });
        context.Solutions.Add(new Solution { Content = "B", AnonymousComplaintId = 1, Archived = true });
        await context.SaveChangesAsync();

        var result = await repo.GetNonArchivedByComplaintIdAsync(1);

        Assert.Single(result);
    }

    [Fact]
    public async Task ArchiveAsync_Should_Set_Archived_True()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        var solution = new Solution { Content = "A", Archived = false };
        context.Solutions.Add(solution);
        await context.SaveChangesAsync();

        await repo.ArchiveAsync(solution.SolutionId);

        var updated = await context.Solutions.FindAsync(solution.SolutionId);

        Assert.True(updated.Archived);
    }

    [Fact]
    public async Task RestoreAsync_Should_Set_Archived_False()
    {
        var context = TestDbContextFactory.Create();
        var repo = new SolutionRepository(context);

        var solution = new Solution { Content = "A", Archived = true };
        context.Solutions.Add(solution);
        await context.SaveChangesAsync();

        await repo.RestoreAsync(solution.SolutionId);

        var updated = await context.Solutions.FindAsync(solution.SolutionId);

        Assert.False(updated.Archived);
    }
}