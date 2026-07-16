namespace OpeaLibrary.Domain.Interfaces;

public interface IBookWriteRepository
{
    Task AddBookAsync(Book book);    
}