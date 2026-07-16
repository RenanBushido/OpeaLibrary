namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.EventHandlers;

public sealed class BookCreatedEventHandler(IMongoDatabase database, ILogger<BookCreatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<BookCreatedEvent>>
{
    private readonly IMongoCollection<Book> _collection = database.GetCollection<Book>("books");

    public async Task Handle(DomainEventNotification<BookCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var book = Book.Restore(
                domainEvent.BookId,
                domainEvent.Title,
                domainEvent.Author,
                domainEvent.PublishedYear,
                domainEvent.QuantityAvailable);

            var filter = Builders<Book>.Filter.Eq(b => b.Id, domainEvent.BookId);

            await _collection.ReplaceOneAsync(filter, book, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to project BookCreatedEvent for book {BookId} to MongoDB.", domainEvent.BookId);
        }
    }
}
