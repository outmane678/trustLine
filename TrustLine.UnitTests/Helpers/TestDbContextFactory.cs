using Microsoft.EntityFrameworkCore;
using AnonymousComplaintsAPI.Data;

public static class TestDbContextFactory
{
    public static AnonymousComplaintsV002Context Create()
    {
        var options = new DbContextOptionsBuilder<AnonymousComplaintsV002Context>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AnonymousComplaintsV002Context(options);
    }
}

// Alias for AttachmentRepositoryTests which uses "DbContextFactory"
public static class DbContextFactory
{
    public static AnonymousComplaintsV002Context Create()
    {
        return TestDbContextFactory.Create();
    }
}
