namespace OpeaLibrary.Infrastructure.Tests.Repositories;

[Collection(EphemeralMongoCollection.Name)]
public class LoanReadRepositoryTests(EphemeralMongoFixture fixture)
{
    private LoanReadRepository CreateSut(out IMongoCollection<Loan> collection)
    {
        var database = fixture.CreateIsolatedDatabase();
        collection = database.GetCollection<Loan>("loans");
        return new LoanReadRepository(database);
    }

    [Fact]
    public async Task GetAllLoansAsync_WhenNoLoansSeeded_ReturnsEmptyCollection()
    {
        var sut = CreateSut(out _);

        var result = await sut.GetAllLoansAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllLoansAsync_WhenOneLoanSeeded_ReturnsSingleItemCollection()
    {
        var sut = CreateSut(out var collection);
        var loan = Loan.Create(Guid.NewGuid());
        await collection.InsertOneAsync(loan);

        var result = await sut.GetAllLoansAsync(CancellationToken.None);

        var single = Assert.Single(result);
        Assert.Equal(loan.Id, single.Id);
        Assert.Equal(loan.BookId, single.BookId);
        Assert.Equal(loan.Status, single.Status);
    }

    [Fact]
    public async Task GetAllLoansAsync_WhenMultipleLoansSeeded_ReturnsAllLoans()
    {
        var sut = CreateSut(out var collection);
        var loan1 = Loan.Create(Guid.NewGuid());
        var loan2 = Loan.Create(Guid.NewGuid());
        await collection.InsertManyAsync([loan1, loan2]);

        var result = await sut.GetAllLoansAsync(CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.Id == loan1.Id);
        Assert.Contains(result, l => l.Id == loan2.Id);
    }
}
