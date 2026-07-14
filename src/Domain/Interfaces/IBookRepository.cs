namespace OpeaLibrary.Domain.Interfaces
{
    public interface IBookRepository
    {
        Task AddBookAsync(Book book);
        Task<Book> GetBookByIdAsync(int id);
        Task<IEnumerable<Book>> GetAllBooksAsync();
    }
}