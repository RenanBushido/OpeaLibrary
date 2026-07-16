namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.EventHandlers;

public sealed class LoanReturnedEventHandler(IMongoDatabase database, ILogger<LoanReturnedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<LoanReturnedEvent>>
{
    private readonly IMongoCollection<Loan> _collection = database.GetCollection<Loan>("loans");

    public async Task Handle(DomainEventNotification<LoanReturnedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var filter = Builders<Loan>.Filter.Eq(l => l.Id, domainEvent.LoanId);
            var update = Builders<Loan>.Update
                .Set(l => l.ReturnDate, domainEvent.ReturnDate)
                .Set(l => l.Status, StatusLoan.Returned);

            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to project LoanReturnedEvent for loan {LoanId} to MongoDB.", domainEvent.LoanId);
        }
    }
}
