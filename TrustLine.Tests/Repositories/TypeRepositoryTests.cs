using Xunit;
using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data;
using AnonymousComplaintsAPI.Repositories.Implementations;
using AnonymousComplaintsAPI.Models.Entities;

public class TypeRepositoryTests
{
    private AnonymousComplaintsV002Context GetContext()
    {
        var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AnonymousComplaintsV002Context(options);
    }

    [Fact]
    public async Task GetNonArchivedAsync_Should_Return_Only_Active()
    {
        var context = GetContext();

        context.Types.Add(new AnonymousComplaintsAPI.Models.Entities.Type
        {
            Name = "Type1",
            Archived = false
        });

        context.Types.Add(new AnonymousComplaintsAPI.Models.Entities.Type
        {
            Name = "Type2",
            Archived = true
        });

        await context.SaveChangesAsync();

        var repo = new TypeRepository(context);

        var result = await repo.GetNonArchivedAsync();

        Assert.Single(result);
    }
}