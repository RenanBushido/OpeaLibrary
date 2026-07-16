namespace OpeaLibrary.Infrastructure.Persistence.Postgres.Repositories;

public sealed class BookWriteRepository(OpeaLibraryDbContext dbContext) : IBookWriteRepository
{
    private readonly OpeaLibraryDbContext _dbContext = dbContext;

    public async Task AddBookAsync(Book book)
    {
        await _dbContext.Books.AddAsync(book);        
    }
}