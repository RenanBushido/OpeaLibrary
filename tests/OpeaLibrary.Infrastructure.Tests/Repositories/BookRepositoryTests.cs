namespace OpeaLibrary.Infrastructure.Tests.Repositories;

public class BookRepositoryTests
{
    [Fact]
    public async Task AddBookAsync_PersistsBook_RetrievableAfterSave()
    {
        using var dbContext = TestDbContextFactory.Create();
        var repository = new BookRepository(dbContext);
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);

        await repository.AddBookAsync(book);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.Books.FindAsync(book.Id);
        Assert.NotNull(persisted);
        Assert.Equal(book.Title, persisted!.Title);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ReturnsBook()
    {
        using var dbContext = TestDbContextFactory.Create();
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);
        await dbContext.Books.AddAsync(book);
        await dbContext.SaveChangesAsync();

        var repository = new BookRepository(dbContext);
        var result = await repository.GetBookByIdAsync(book.Id);

        Assert.NotNull(result);
        Assert.Equal(book.Id, result!.Id);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        using var dbContext = TestDbContextFactory.Create();
        var repository = new BookRepository(dbContext);

        var result = await repository.GetBookByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenNoBooksPersisted_ReturnsEmptyCollection()
    {
        using var dbContext = TestDbContextFactory.Create();
        var repository = new BookRepository(dbContext);

        var result = await repository.GetAllBooksAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenBooksPersisted_ReturnsAllBooks()
    {
        using var dbContext = TestDbContextFactory.Create();
        var book1 = Book.Create("Clean Code", "Robert C. Martin", 2008);
        var book2 = Book.Create("The Pragmatic Programmer", "Andy Hunt", 1999);
        await dbContext.Books.AddRangeAsync(book1, book2);
        await dbContext.SaveChangesAsync();

        var repository = new BookRepository(dbContext);
        var result = await repository.GetAllBooksAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, b => b.Id == book1.Id);
        Assert.Contains(result, b => b.Id == book2.Id);
    }
}
