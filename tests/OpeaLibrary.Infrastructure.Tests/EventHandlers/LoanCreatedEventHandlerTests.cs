using OpeaLibrary.Infrastructure.Tests.Repositories;

namespace OpeaLibrary.Infrastructure.Tests.EventHandlers;

[Collection(EphemeralMongoCollection.Name)]
public class LoanCreatedEventHandlerTests(EphemeralMongoFixture fixture)
{
    private static IMongoDatabase CreateUnreachableDatabase()
    {
        var settings = MongoClientSettings.FromConnectionString("mongodb://localhost:1/");
        settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(200);

        return new MongoClient(settings).GetDatabase("unreachable");
    }

    [Fact]
    public async Task Handle_UpsertsNewLoanDocument()
    {
        var database = fixture.CreateIsolatedDatabase();
        var sut = new LoanCreatedEventHandler(database, NullLogger<LoanCreatedEventHandler>.Instance);
        var loanId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var loanDate = DateTime.UtcNow;
        var domainEvent = new LoanCreatedEvent(loanId, bookId, loanDate);

        await sut.Handle(new DomainEventNotification<LoanCreatedEvent>(domainEvent), CancellationToken.None);

        var collection = database.GetCollection<Loan>("loans");
        var persisted = await collection.Find(Builders<Loan>.Filter.Eq(l => l.Id, loanId)).SingleOrDefaultAsync();
        Assert.NotNull(persisted);
        Assert.Equal(bookId, persisted!.BookId);
        Assert.True((loanDate - persisted.LoanDate).Duration() < TimeSpan.FromMilliseconds(5));
        Assert.Null(persisted.ReturnDate);
        Assert.Equal(StatusLoan.Active, persisted.Status);
    }

    [Fact]
    public async Task Handle_WhenMongoWriteFails_DoesNotThrow()
    {
        var sut = new LoanCreatedEventHandler(CreateUnreachableDatabase(), NullLogger<LoanCreatedEventHandler>.Instance);
        var domainEvent = new LoanCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

        await sut.Handle(new DomainEventNotification<LoanCreatedEvent>(domainEvent), CancellationToken.None);
    }
}
