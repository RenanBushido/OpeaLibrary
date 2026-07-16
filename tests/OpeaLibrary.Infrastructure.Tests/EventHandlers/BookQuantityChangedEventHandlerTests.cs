using OpeaLibrary.Infrastructure.Tests.Repositories;

namespace OpeaLibrary.Infrastructure.Tests.EventHandlers;

[Collection(EphemeralMongoCollection.Name)]
public class BookQuantityChangedEventHandlerTests(EphemeralMongoFixture fixture)
{
    private static IMongoDatabase CreateUnreachableDatabase()
    {
        var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:1/");
        settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(200);

        return new MongoClient(settings).GetDatabase("unreachable");
    }

    [Fact]
    public async Task Handle_UpdatesExistingBookQuantity()
    {
        var database = fixture.CreateIsolatedDatabase();
        var collection = database.GetCollection<Book>("books");
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008, 3);
        await collection.InsertOneAsync(book);
        var sut = new BookQuantityChangedEventHandler(database, NullLogger<BookQuantityChangedEventHandler>.Instance);

        await sut.Handle(new DomainEventNotification<BookQuantityChangedEvent>(new BookQuantityChangedEvent(book.Id, 7)), CancellationToken.None);

        var persisted = await collection.Find(Builders<Book>.Filter.Eq(b => b.Id, book.Id)).SingleOrDefaultAsync();
        Assert.Equal(7, persisted!.QuantityAvailable);
        Assert.Equal(book.Title, persisted.Title);
    }

    [Fact]
    public async Task Handle_WhenMongoWriteFails_DoesNotThrow()
    {
        var sut = new BookQuantityChangedEventHandler(CreateUnreachableDatabase(), NullLogger<BookQuantityChangedEventHandler>.Instance);
        var domainEvent = new BookQuantityChangedEvent(Guid.NewGuid(), 1);

        await sut.Handle(new DomainEventNotification<BookQuantityChangedEvent>(domainEvent), CancellationToken.None);
    }
}
