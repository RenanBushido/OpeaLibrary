namespace OpeaLibrary.Domain.Interfaces;

public interface IBookReadRepository
{
    Task<Book?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Book>> GetAllBooksAsync(CancellationToken cancellationToken);
}