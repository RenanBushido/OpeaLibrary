namespace OpeaLibrary.Domain.Entities;

public sealed class Loan : Entity
{
    public Guid BookId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime LoanDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }
    public StatusLoan Status { get; private set; }

    public static Loan Create(Guid bookId, Guid userId)
    {
        if (bookId == Guid.Empty)
            throw new DomainException("Book ID cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("User ID cannot be empty.");

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            UserId = userId,
            LoanDate = DateTime.UtcNow,
            ReturnDate = null,
            Status = StatusLoan.Active
        };

        return loan;
    }

    public void MarkAsReturned()
    {
        if (ReturnDate != null)
            throw new DomainException("This loan has already been returned.");

        ReturnDate = DateTime.UtcNow;
        Status = StatusLoan.Returned;
    }
}