namespace OpeaLibrary.Infrastructure.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task Repositories_ShareTheSameDbContext()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);

        Assert.NotNull(unitOfWork.BookRepository);
        Assert.NotNull(unitOfWork.LoanRepository);

        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);
        await unitOfWork.BookRepository.AddBookAsync(book);

        var trackedBook = await dbContext.Books.FindAsync(book.Id);
        Assert.NotNull(trackedBook);
        Assert.Equal(book.Id, trackedBook!.Id);
    }

    [Fact]
    public async Task CommitAsync_PersistsStagedChanges()
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
}
