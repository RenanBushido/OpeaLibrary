namespace OpeaLibrary.Infrastructure.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task Repositories_ShareTheSameDbContext()
    {
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;
        var unitOfWork = new UnitOfWork(dbContext);

        Assert.NotNull(unitOfWork.BookWriteRepository);
        Assert.NotNull(unitOfWork.LoanWriteRepository);

        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);
        await unitOfWork.BookWriteRepository.AddBookAsync(book);

        var trackedBook = await dbContext.Books.FindAsync(book.Id);
        Assert.NotNull(trackedBook);
        Assert.Equal(book.Id, trackedBook!.Id);
    }

    [Fact]
    public async Task CommitAsync_PersistsStagedChanges()
    {
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;
        var unitOfWork = new UnitOfWork(dbContext);
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);

        await unitOfWork.BookWriteRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        var persisted = await dbContext.Books.FindAsync(book.Id);
        Assert.NotNull(persisted);
        Assert.Equal(book.Title, persisted!.Title);
    }
}
