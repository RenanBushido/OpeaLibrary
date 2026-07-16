namespace OpeaLibrary.Infrastructure.Tests.Repositories;

[Collection(EphemeralMongoCollection.Name)]
public class BookReadRepositoryTests(EphemeralMongoFixture fixture)
{
    private BookReadRepository CreateSut(out IMongoCollection<Book> collection)
    {
        var database = fixture.CreateIsolatedDatabase();
        collection = database.GetCollection<Book>("books");
        return new BookReadRepository(database);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ReturnsBook()
    {
        var sut = CreateSut(out var collection);
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008, 3);
        await collection.InsertOneAsync(book);

        var result = await sut.GetBookByIdAsync(book.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(book.Id, result!.Id);
        Assert.Equal(book.Title, result.Title);
        Assert.Equal(book.Author, result.Author);
        Assert.Equal(book.PublishedYear, result.PublishedYear);
        Assert.Equal(book.QuantityAvailable, result.QuantityAvailable);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        var sut = CreateSut(out _);

        var result = await sut.GetBookByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenNoBooksSeeded_ReturnsEmptyCollection()
    {
        var sut = CreateSut(out _);

        var result = await sut.GetAllBooksAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenOneBookSeeded_ReturnsSingleItemCollection()
    {
        var sut = CreateSut(out var collection);
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);
        await collection.InsertOneAsync(book);

        var result = await sut.GetAllBooksAsync(CancellationToken.None);

        var single = Assert.Single(result);
        Assert.Equal(book.Id, single.Id);
    }

    [Fact]
    public async Task GetAllBooksAsync_WhenMultipleBooksSeeded_ReturnsAllBooks()
    {
        var sut = CreateSut(out var collection);
        var book1 = Book.Create("Clean Code", "Robert C. Martin", 2008);
        var book2 = Book.Create("The Pragmatic Programmer", "Andy Hunt", 1999);
        await collection.InsertManyAsync([book1, book2]);

        var result = await sut.GetAllBooksAsync(CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, b => b.Id == book1.Id);
        Assert.Contains(result, b => b.Id == book2.Id);
    }
}
