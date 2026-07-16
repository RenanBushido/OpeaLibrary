namespace OpeaLibrary.Domain.Interfaces;

public interface IUnitOfWork
{
    IBookWriteRepository BookWriteRepository { get; }
    ILoanWriteRepository LoanWriteRepository { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
}