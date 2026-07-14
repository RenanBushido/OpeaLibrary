namespace OpeaLibrary.Domain.Interfaces
{
    public interface ILoanRepository
    {
        Task<bool> RequestLoanAsync(int bookId, int userId);
        Task<bool> ReturnLoanAsync(int loanId);
        Task<IEnumerable<Loan>> GetAllLoansAsync();
    }
}