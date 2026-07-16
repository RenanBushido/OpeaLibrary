namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.EventHandlers;

public sealed class BookQuantityChangedEventHandler(IMongoDatabase database, ILogger<BookQuantityChangedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<BookQuantityChangedEvent>>
{
    private readonly IMongoCollection<Book> _collection = database.GetCollection<Book>("books");

    public async Task Handle(DomainEventNotification<BookQuantityChangedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var filter = Builders<Book>.Filter.Eq(b => b.Id, domainEvent.BookId);
            var update = Builders<Book>.Update.Set(b => b.QuantityAvailable, domainEvent.QuantityAvailable);

            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to project BookQuantityChangedEvent for book {BookId} to MongoDB.", domainEvent.BookId);
        }
    }
}
