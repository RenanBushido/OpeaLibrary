namespace OpeaLibrary.Domain.Interfaces;

public interface ILoanWriteRepository
{
    Task<bool> RequestLoanAsync(Guid bookId);
    Task<bool> ReturnLoanAsync(Guid loanId);
}