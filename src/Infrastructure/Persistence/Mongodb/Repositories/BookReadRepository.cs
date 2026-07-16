namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.Repositories;

public sealed class BookReadRepository(IMongoDatabase database) : IBookReadRepository
{
    private readonly IMongoCollection<Book> _collection = database.GetCollection<Book>("books");

    public async Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(Builders<Book>.Filter.Empty).ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<Book>.Filter.Eq(b => b.Id, id);

        return await _collection.Find(filter).SingleOrDefaultAsync(cancellationToken);
    }
    
}