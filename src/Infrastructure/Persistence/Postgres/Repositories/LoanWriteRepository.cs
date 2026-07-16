namespace OpeaLibrary.Infrastructure.Persistence.Postgres.Repositories;

public class LoanWriteRepository(OpeaLibraryDbContext dbContext) : ILoanWriteRepository
{
    private readonly OpeaLibraryDbContext _dbContext = dbContext;

    public async Task<bool> RequestLoanAsync(Guid bookId)
    {
        var book = await _dbContext.Books.FindAsync(bookId);

        if (book == null || book.QuantityAvailable <= 0) return false;        

        book.DecreaseQuantity();

        var loan = Loan.Create(bookId);

        await _dbContext.Loans.AddAsync(loan);

        return true;
    }

    public async Task<bool> ReturnLoanAsync(Guid loanId)
    {
        var loan = await _dbContext.Loans.FindAsync(loanId);

        if (loan == null || loan.ReturnDate.HasValue) return false;

        loan.MarkAsReturned();

        var book = await _dbContext.Books.FindAsync(loan.BookId);

        book?.IncreaseQuantity();

        return true;
    }
}