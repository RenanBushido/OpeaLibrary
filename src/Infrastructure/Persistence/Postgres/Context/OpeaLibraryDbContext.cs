namespace OpeaLibrary.Infrastructure.Persistence.Postgres.Context;

public sealed class OpeaLibraryDbContext(DbContextOptions<OpeaLibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books {get; set;}
    public DbSet<Loan> Loans {get; set;}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpeaLibraryDbContext).Assembly);
    }
}