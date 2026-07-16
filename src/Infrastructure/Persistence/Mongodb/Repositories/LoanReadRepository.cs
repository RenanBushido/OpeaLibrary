namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.Repositories;

public sealed class LoanReadRepository(IMongoDatabase database) : ILoanReadRepository
{
    private readonly IMongoCollection<Loan> _collection = database.GetCollection<Loan>("loans");

    public async Task<IEnumerable<Loan>> GetAllLoansAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(Builders<Loan>.Filter.Empty).ToListAsync(cancellationToken);
    }
}