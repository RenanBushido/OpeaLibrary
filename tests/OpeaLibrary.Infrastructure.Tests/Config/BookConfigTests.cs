namespace OpeaLibrary.Infrastructure.Tests.Config;

public class BookConfigTests
{
    [Fact]
    public void BookEntity_IsConfiguredWithExpectedTableAndKey()
    {
        using var dbContext = TestDbContextFactory.Create();

        var entityType = dbContext.Model.FindEntityType(typeof(Book));

        Assert.NotNull(entityType);
        Assert.Equal("tb_books", entityType!.GetTableName());
        Assert.Equal(nameof(Book.Id), Assert.Single(entityType.FindPrimaryKey()!.Properties).Name);
    }

    [Fact]
    public void BookEntity_TitleIsRequiredWithMaxLength200()
    {
        using var dbContext = TestDbContextFactory.Create();
        var entityType = dbContext.Model.FindEntityType(typeof(Book));

        var title = entityType!.FindProperty(nameof(Book.Title));

        Assert.NotNull(title);
        Assert.False(title!.IsNullable);
        Assert.Equal(200, title.GetMaxLength());
    }

    [Fact]
    public void BookEntity_AuthorIsRequiredWithMaxLength100()
    {
        using var dbContext = TestDbContextFactory.Create();
        var entityType = dbContext.Model.FindEntityType(typeof(Book));

        var author = entityType!.FindProperty(nameof(Book.Author));

        Assert.NotNull(author);
        Assert.False(author!.IsNullable);
        Assert.Equal(100, author.GetMaxLength());
    }

    [Fact]
    public void BookEntity_PublishedYearAndQuantityAvailableAreRequired()
    {
        using var dbContext = TestDbContextFactory.Create();
        var entityType = dbContext.Model.FindEntityType(typeof(Book));

        var publishedYear = entityType!.FindProperty(nameof(Book.PublishedYear));
        var quantityAvailable = entityType.FindProperty(nameof(Book.QuantityAvailable));

        Assert.NotNull(publishedYear);
        Assert.NotNull(quantityAvailable);
        Assert.False(publishedYear!.IsNullable);
        Assert.False(quantityAvailable!.IsNullable);
    }
}
