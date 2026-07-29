using Xunit;
using AnonymousComplaintsAPI.Repositories.Implementations;
using AnonymousComplaintsAPI.Models.Entities;
using System.Linq;
using System.Threading.Tasks;

public class CategoryRepositoryTests
{
    [Fact]
    public async Task CreateAsync_Should_Add_Category()
    {
        // Arrange
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        var category = new Category
        {
            Name = "IT",
            TypeId = 1,
            Archived = false
        };

        // Act
        var result = await repo.CreateAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("IT", result.Name);
        Assert.Single(context.Categories);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Category()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        var category = new Category { Name = "HR" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(category.CategoryId);

        Assert.NotNull(result);
        Assert.Equal("HR", result.Name);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        context.Categories.Add(new Category { Name = "A" });
        context.Categories.Add(new Category { Name = "B" });
        await context.SaveChangesAsync();

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task UpdateAsync_Should_Modify_Category()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        var category = new Category { Name = "Old Name" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        category.Name = "New Name";

        await repo.UpdateAsync(category);

        var updated = await context.Categories.FindAsync(category.CategoryId);

        Assert.Equal("New Name", updated.Name);
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Category()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        var category = new Category { Name = "ToDelete" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(category.CategoryId);

        Assert.Empty(context.Categories);
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        var category = new Category { Name = "Test" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var result = await repo.ExistsAsync(category.CategoryId);

        Assert.True(result);
    }

    [Fact]
    public async Task GetByTypeIdAsync_Should_Filter()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        context.Categories.Add(new Category { Name = "A", TypeId = 1 });
        context.Categories.Add(new Category { Name = "B", TypeId = 2 });
        await context.SaveChangesAsync();

        var result = await repo.GetByTypeIdAsync(1);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetNonArchivedAsync_Should_Filter_Archived()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        context.Categories.Add(new Category { Name = "A", Archived = false });
        context.Categories.Add(new Category { Name = "B", Archived = true });
        await context.SaveChangesAsync();

        var result = await repo.GetNonArchivedAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task ArchiveAsync_Should_Set_Archived_True()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        var category = new Category { Name = "A", Archived = false };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        await repo.ArchiveAsync(category.CategoryId);

        var updated = await context.Categories.FindAsync(category.CategoryId);

        Assert.True(updated.Archived);
    }

    [Fact]
    public async Task RestoreAsync_Should_Set_Archived_False()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        var category = new Category { Name = "A", Archived = true };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        await repo.RestoreAsync(category.CategoryId);

        var updated = await context.Categories.FindAsync(category.CategoryId);

        Assert.False(updated.Archived);
    }

    [Fact]
    public async Task GetAllWithTypeAsync_Should_Return_Including_Type()
    {
        var context = TestDbContextFactory.Create();
        var repo = new CategoryRepository(context);

        context.Categories.Add(new Category
        {
            Name = "A",
            Type = new AnonymousComplaintsAPI.Models.Entities.Type
            {
                Name = "Type1"
            }
        });

        await context.SaveChangesAsync();

        var result = await repo.GetAllWithTypeAsync();

        Assert.Single(result);
        Assert.NotNull(result.First().Type);
    }
}