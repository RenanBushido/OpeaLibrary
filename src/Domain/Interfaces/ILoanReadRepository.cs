namespace OpeaLibrary.Domain.Interfaces;

public interface ILoanReadRepository
{    
    Task<IEnumerable<Loan>> GetAllLoansAsync(CancellationToken cancellationToken);
}