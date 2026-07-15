namespace OpeaLibrary.Infrastructure.Tests;

internal static class TestDbContextFactory
{
    public static OpeaLibraryDbContext Create()
    {
        var options = new DbContextOptionsBuilder<OpeaLibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OpeaLibraryDbContext(options);
    }
}
