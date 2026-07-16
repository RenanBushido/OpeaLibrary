using OpeaLibrary.Infrastructure.Tests.Repositories;

namespace OpeaLibrary.Infrastructure.Tests.EventHandlers;

[Collection(EphemeralMongoCollection.Name)]
public class LoanReturnedEventHandlerTests(EphemeralMongoFixture fixture)
{
    private static IMongoDatabase CreateUnreachableDatabase()
    {
        var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:1/");
        settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(200);

        return new MongoClient(settings).GetDatabase("unreachable");
    }

    [Fact]
    public async Task Handle_UpdatesExistingLoanReturnState()
    {
        var database = fixture.CreateIsolatedDatabase();
        var collection = database.GetCollection<Loan>("loans");
        var loan = Loan.Create(Guid.NewGuid());
        await collection.InsertOneAsync(loan);
        var sut = new LoanReturnedEventHandler(database, NullLogger<LoanReturnedEventHandler>.Instance);
        var returnDate = DateTime.UtcNow;

        await sut.Handle(new DomainEventNotification<LoanReturnedEvent>(new LoanReturnedEvent(loan.Id, returnDate)), CancellationToken.None);

        var persisted = await collection.Find(Builders<Loan>.Filter.Eq(l => l.Id, loan.Id)).SingleOrDefaultAsync();
        Assert.NotNull(persisted!.ReturnDate);
        Assert.True((returnDate - persisted.ReturnDate!.Value).Duration() < TimeSpan.FromMilliseconds(5));
        Assert.Equal(StatusLoan.Returned, persisted.Status);
        Assert.Equal(loan.BookId, persisted.BookId);
    }

    [Fact]
    public async Task Handle_WhenMongoWriteFails_DoesNotThrow()
    {
        var sut = new LoanReturnedEventHandler(CreateUnreachableDatabase(), NullLogger<LoanReturnedEventHandler>.Instance);
        var domainEvent = new LoanReturnedEvent(Guid.NewGuid(), DateTime.UtcNow);

        await sut.Handle(new DomainEventNotification<LoanReturnedEvent>(domainEvent), CancellationToken.None);
    }
}
