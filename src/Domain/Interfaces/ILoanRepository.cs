namespace OpeaLibrary.Domain.Interfaces;

public interface ILoanRepository
{
    Task<bool> RequestLoanAsync(Guid bookId);
    Task<bool> ReturnLoanAsync(Guid loanId);
    Task<IEnumerable<Loan>> GetAllLoansAsync();
}