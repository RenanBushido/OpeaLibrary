namespace OpeaLibrary.Infrastructure.Persistence.Postgres.Repositories;

public sealed class BookRepository(OpeaLibraryDbContext dbContext) : IBookRepository
{
    private readonly OpeaLibraryDbContext _dbContext = dbContext;

    public async Task AddBookAsync(Book book)
    {
        await _dbContext.Books.AddAsync(book);        
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _dbContext.Books.ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(Guid id)
    {
        return await _dbContext.Books.FindAsync(id);
    }
}