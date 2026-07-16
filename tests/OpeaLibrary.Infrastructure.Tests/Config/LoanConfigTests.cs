namespace OpeaLibrary.Infrastructure.Tests.Config;

public class LoanConfigTests
{
    [Fact]
    public void LoanEntity_IsConfiguredWithExpectedTableAndKey()
    {
        using var dbContext = TestDbContextFactory.Create();

        var entityType = dbContext.Model.FindEntityType(typeof(Loan));

        Assert.NotNull(entityType);
        Assert.Equal("tb_loans", entityType!.GetTableName());
        Assert.Equal(nameof(Loan.Id), Assert.Single(entityType.FindPrimaryKey()!.Properties).Name);
    }

    [Fact]
    public void LoanEntity_BookIdAndLoanDateAreRequired()
    {
        using var dbContext = TestDbContextFactory.Create();
        var entityType = dbContext.Model.FindEntityType(typeof(Loan));

        var bookId = entityType!.FindProperty(nameof(Loan.BookId));
        var loanDate = entityType.FindProperty(nameof(Loan.LoanDate));

        Assert.NotNull(bookId);
        Assert.NotNull(loanDate);
        Assert.False(bookId!.IsNullable);
        Assert.False(loanDate!.IsNullable);
    }

    [Fact]
    public void LoanEntity_ReturnDateIsOptional()
    {
        using var dbContext = TestDbContextFactory.Create();
        var entityType = dbContext.Model.FindEntityType(typeof(Loan));

        var returnDate = entityType!.FindProperty(nameof(Loan.ReturnDate));

        Assert.NotNull(returnDate);
        Assert.True(returnDate!.IsNullable);
    }

    [Fact]
    public void LoanEntity_HasForeignKeyRelationshipToBook()
    {
        using var dbContext = TestDbContextFactory.Create();
        var entityType = dbContext.Model.FindEntityType(typeof(Loan));

        var foreignKey = Assert.Single(entityType!.GetForeignKeys());

        Assert.Equal(typeof(Book), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(Loan.BookId), Assert.Single(foreignKey.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }
}
