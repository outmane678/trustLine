using Xunit;
using AnonymousComplaintsAPI.Repositories.Implementations;
using AnonymousComplaintsAPI.Models.Entities;
using System.Linq;
using System.Threading.Tasks;

public class FrequencyRepositoryTests
{
    [Fact]
    public async Task CreateAsync_Should_Add_Frequency()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        var frequency = new Frequency
        {
            Label = "Daily",
            Archived = false
        };

        var result = await repo.CreateAsync(frequency);

        Assert.NotNull(result);
        Assert.Equal("Daily", result.Label);
        Assert.Single(context.Frequencies);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Frequency()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        var freq = new Frequency { Label = "Weekly" };
        context.Frequencies.Add(freq);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(freq.FrequencyId);

        Assert.NotNull(result);
        Assert.Equal("Weekly", result.Label);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        context.Frequencies.Add(new Frequency { Label = "A" });
        context.Frequencies.Add(new Frequency { Label = "B" });
        await context.SaveChangesAsync();

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_Should_Modify_Frequency()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        var freq = new Frequency { Label = "Old" };
        context.Frequencies.Add(freq);
        await context.SaveChangesAsync();

        freq.Label = "New";

        await repo.UpdateAsync(freq);

        var updated = await context.Frequencies.FindAsync(freq.FrequencyId);

        Assert.Equal("New", updated.Label);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Frequency()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        var freq = new Frequency { Label = "ToDelete" };
        context.Frequencies.Add(freq);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(freq.FrequencyId);

        Assert.Empty(context.Frequencies);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        var freq = new Frequency { Label = "Test" };
        context.Frequencies.Add(freq);
        await context.SaveChangesAsync();

        var result = await repo.ExistsAsync(freq.FrequencyId);

        Assert.True(result);
    }

    [Fact]
    public async Task GetNonArchivedAsync_Should_Filter_Archived()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        context.Frequencies.Add(new Frequency { Label = "A", Archived = false });
        context.Frequencies.Add(new Frequency { Label = "B", Archived = true });
        await context.SaveChangesAsync();

        var result = await repo.GetNonArchivedAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task ArchiveAsync_Should_Set_Archived_True()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        var freq = new Frequency { Label = "A", Archived = false };
        context.Frequencies.Add(freq);
        await context.SaveChangesAsync();

        await repo.ArchiveAsync(freq.FrequencyId);

        var updated = await context.Frequencies.FindAsync(freq.FrequencyId);

        Assert.True(updated.Archived);
    }

    [Fact]
    public async Task RestoreAsync_Should_Set_Archived_False()
    {
        var context = TestDbContextFactory.Create();
        var repo = new FrequencyRepository(context);

        var freq = new Frequency { Label = "A", Archived = true };
        context.Frequencies.Add(freq);
        await context.SaveChangesAsync();

        await repo.RestoreAsync(freq.FrequencyId);

        var updated = await context.Frequencies.FindAsync(freq.FrequencyId);

        Assert.False(updated.Archived);
    }
}