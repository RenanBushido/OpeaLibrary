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
    public static SqliteTestDatabase Create(params IInterceptor[] interceptors)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var optionsBuilder = new DbContextOptionsBuilder<OpeaLibraryDbContext>()
            .UseSqlite(connection);

        if (interceptors.Length > 0)
        {
            optionsBuilder.AddInterceptors(interceptors);
        }

        var context = new OpeaLibraryDbContext(optionsBuilder.Options);
        context.Database.EnsureCreated();

        return new SqliteTestDatabase(connection, context);
    }
}
