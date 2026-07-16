namespace OpeaLibrary.Infrastructure.Tests.Repositories;

public class BookRepositoryTests
{
    [Fact]
    public async Task AddBookAsync_PersistsBook_RetrievableAfterSave()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);

        await unitOfWork.BookRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        var persisted = await unitOfWork.BookRepository.GetBookByIdAsync(book.Id, CancellationToken.None);
        Assert.NotNull(persisted);
        Assert.Equal(book.Title, persisted!.Title);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ReturnsBook()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);
        await unitOfWork.BookRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        var result = await unitOfWork.BookRepository.GetBookByIdAsync(book.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(book.Id, result!.Id);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);

        var result = await unitOfWork.BookRepository.GetBookByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenNoBooksPersisted_ReturnsEmptyCollection()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);

        var result = await unitOfWork.BookRepository.GetAllBooksAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenBooksPersisted_ReturnsAllBooks()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var book1 = Book.Create("Clean Code", "Robert C. Martin", 2008);
        var book2 = Book.Create("The Pragmatic Programmer", "Andy Hunt", 1999);
        await unitOfWork.BookRepository.AddBookAsync(book1);
        await unitOfWork.BookRepository.AddBookAsync(book2);
        await unitOfWork.CommitAsync();

        var result = await unitOfWork.BookRepository.GetAllBooksAsync(CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, b => b.Id == book1.Id);
        Assert.Contains(result, b => b.Id == book2.Id);
    }
}
