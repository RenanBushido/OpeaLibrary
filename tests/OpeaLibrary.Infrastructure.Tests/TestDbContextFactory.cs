namespace OpeaLibrary.Infrastructure.Tests;

internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public OpeaLibraryDbContext Context { get; }

    public SqliteTestDatabase(SqliteConnection connection, OpeaLibraryDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}

internal static class TestDbContextFactory
{
    public static SqliteTestDatabase Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<OpeaLibraryDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new OpeaLibraryDbContext(options);
        context.Database.EnsureCreated();

        return new SqliteTestDatabase(connection, context);
    }
}
