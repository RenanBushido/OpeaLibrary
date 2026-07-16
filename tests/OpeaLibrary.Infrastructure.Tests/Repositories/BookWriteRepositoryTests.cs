namespace OpeaLibrary.Infrastructure.Tests.Repositories;

public class BookWriteRepositoryTests
{
    [Fact]
    public async Task AddBookAsync_PersistsBook_RetrievableAfterSave()
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
