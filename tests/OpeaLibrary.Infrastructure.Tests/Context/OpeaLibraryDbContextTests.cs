namespace OpeaLibrary.Infrastructure.Tests.Context;

public class OpeaLibraryDbContextTests
{
    [Fact]
    public void DbSets_AreInitialized()
    {
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;

        Assert.NotNull(dbContext.Books);
        Assert.NotNull(dbContext.Loans);
    }

    [Fact]
    public void OnModelCreating_AppliesConfigurationsForBookAndLoan()
    {
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;

        Assert.NotNull(dbContext.Model.FindEntityType(typeof(Book)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(Loan)));
    }
}
