using OpeaLibrary.Infrastructure.Tests.Repositories;

namespace OpeaLibrary.Infrastructure.Tests.EventHandlers;

[Collection(EphemeralMongoCollection.Name)]
public class BookCreatedEventHandlerTests(EphemeralMongoFixture fixture)
{
    private static IMongoDatabase CreateUnreachableDatabase()
    {
        var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:1/");
        settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(200);

        return new MongoClient(settings).GetDatabase("unreachable");
    }

    [Fact]
    public async Task Handle_UpsertsNewBookDocument()
    {
        var database = fixture.CreateIsolatedDatabase();
        var sut = new BookCreatedEventHandler(database, NullLogger<BookCreatedEventHandler>.Instance);
        var bookId = Guid.NewGuid();
        var domainEvent = new BookCreatedEvent(bookId, "Clean Code", "Robert C. Martin", 2008, 3);

        await sut.Handle(new DomainEventNotification<BookCreatedEvent>(domainEvent), CancellationToken.None);

        var collection = database.GetCollection<Book>("books");
        var persisted = await collection.Find(Builders<Book>.Filter.Eq(b => b.Id, bookId)).SingleOrDefaultAsync();
        Assert.NotNull(persisted);
        Assert.Equal("Clean Code", persisted!.Title);
        Assert.Equal("Robert C. Martin", persisted.Author);
        Assert.Equal(2008, persisted.PublishedYear);
        Assert.Equal(3, persisted.QuantityAvailable);
    }

    [Fact]
    public async Task Handle_WhenMongoWriteFails_DoesNotThrow()
    {
        var sut = new BookCreatedEventHandler(CreateUnreachableDatabase(), NullLogger<BookCreatedEventHandler>.Instance);
        var domainEvent = new BookCreatedEvent(Guid.NewGuid(), "Clean Code", "Robert C. Martin", 2008, 3);

        await sut.Handle(new DomainEventNotification<BookCreatedEvent>(domainEvent), CancellationToken.None);
    }
}
