namespace OpeaLibrary.Infrastructure.Tests.Context;

public class OpeaLibraryDbContextTests
{
    [Fact]
    public void DbSets_AreInitialized()
    {
        using var dbContext = TestDbContextFactory.Create();

        Assert.NotNull(dbContext.Books);
        Assert.NotNull(dbContext.Loans);
    }

    [Fact]
    public void OnModelCreating_AppliesConfigurationsForBookAndLoan()
    {
        using var dbContext = TestDbContextFactory.Create();

        Assert.NotNull(dbContext.Model.FindEntityType(typeof(Book)));
        Assert.NotNull(dbContext.Model.FindEntityType(typeof(Loan)));
    }
}
