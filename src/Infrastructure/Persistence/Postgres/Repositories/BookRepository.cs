namespace OpeaLibrary.Infrastructure.Persistence.Postgres.Repositories;

public sealed class BookRepository(OpeaLibraryDbContext dbContext) : IBookRepository
{
    private readonly OpeaLibraryDbContext _dbContext = dbContext;

    public async Task AddBookAsync(Book book)
    {
        await _dbContext.Books.AddAsync(book);        
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Books.ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Books.FindAsync(id, cancellationToken);
    }
}