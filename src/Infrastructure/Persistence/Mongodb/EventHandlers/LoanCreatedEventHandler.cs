namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.EventHandlers;

public sealed class LoanCreatedEventHandler(IMongoDatabase database, ILogger<LoanCreatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<LoanCreatedEvent>>
{
    private readonly IMongoCollection<Loan> _collection = database.GetCollection<Loan>("loans");

    public async Task Handle(DomainEventNotification<LoanCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var loan = Loan.Restore(
                domainEvent.LoanId,
                domainEvent.BookId,
                domainEvent.LoanDate,
                returnDate: null,
                StatusLoan.Active);

            var filter = Builders<Loan>.Filter.Eq(l => l.Id, domainEvent.LoanId);

            await _collection.ReplaceOneAsync(filter, loan, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to project LoanCreatedEvent for loan {LoanId} to MongoDB.", domainEvent.LoanId);
        }
    }
}
