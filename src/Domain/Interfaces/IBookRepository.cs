namespace OpeaLibrary.Domain.Interfaces;

public interface IBookRepository
{
    Task AddBookAsync(Book book);
    Task<Book?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken);
}