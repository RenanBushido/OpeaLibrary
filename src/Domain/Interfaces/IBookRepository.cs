namespace OpeaLibrary.Domain.Interfaces;

public interface IBookRepository
{
    Task AddBookAsync(Book book);
    Task<Book?> GetBookByIdAsync(Guid id);
    Task<IEnumerable<Book>> GetAllBooksAsync();
}