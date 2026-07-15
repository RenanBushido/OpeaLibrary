namespace OpeaLibrary.Domain.Entities;

public sealed class Loan : Entity
{
    public Guid BookId { get; private set; }    
    public DateTime LoanDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }
    public StatusLoan Status { get; private set; }
    private Loan() { }

    public static Loan Create(Guid bookId)
    {
        if (bookId == Guid.Empty)
            throw new DomainException("Book ID cannot be empty.");

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            LoanDate = DateTime.UtcNow,
            ReturnDate = null,
            Status = StatusLoan.Active
        };

        return loan;
    }

    public static Loan Restore(
        Guid id,
        Guid bookId,
        DateTime loanDate,
        DateTime? returnDate,
        StatusLoan status
    )
    {
        return new Loan
        {
            Id = id,
            BookId = bookId,
            LoanDate = loanDate,
            ReturnDate = returnDate,
            Status = status
        };
    }

    public void MarkAsReturned()
    {
        if (ReturnDate != null)
            throw new DomainException("This loan has already been returned.");

        ReturnDate = DateTime.UtcNow;
        Status = StatusLoan.Returned;
    }
}