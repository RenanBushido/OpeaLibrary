namespace OpeaLibrary.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IBookRepository BookRepository { get; }
        ILoanRepository LoanRepository { get; }
        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}