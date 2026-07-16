namespace OpeaLibrary.Domain.Tests.Entities;

public class LoanTests
{
    private static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(5);

    [Fact]
    public void Create_WithValidData_ReturnsLoanWithExpectedValues()
    {
        var bookId = Guid.NewGuid();

        var loan = Loan.Create(bookId);

        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.Equal(bookId, loan.BookId);
        Assert.True(DateTime.UtcNow - loan.LoanDate < Tolerance);
        Assert.Null(loan.ReturnDate);
        Assert.Equal(StatusLoan.Active, loan.Status);
    }

    [Fact]
    public void Create_WithEmptyBookId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() => Loan.Create(Guid.Empty));

        Assert.Equal("Book ID cannot be empty.", exception.Message);
    }

    [Fact]
    public void Create_WithValidData_RaisesLoanCreatedEvent()
    {
        var bookId = Guid.NewGuid();

        var loan = Loan.Create(bookId);

        var domainEvent = Assert.Single(loan.DomainEvents);
        var loanCreatedEvent = Assert.IsType<LoanCreatedEvent>(domainEvent);
        Assert.Equal(loan.Id, loanCreatedEvent.LoanId);
        Assert.Equal(loan.BookId, loanCreatedEvent.BookId);
        Assert.Equal(loan.LoanDate, loanCreatedEvent.LoanDate);
    }

    [Fact]
    public void Restore_WithValidData_ReturnsLoanWithExpectedValues()
    {
        var id = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var loanDate = DateTime.UtcNow.AddDays(-3);
        var returnDate = DateTime.UtcNow.AddDays(-1);

        var loan = Loan.Restore(id, bookId, loanDate, returnDate, StatusLoan.Returned);

        Assert.Equal(id, loan.Id);
        Assert.Equal(bookId, loan.BookId);
        Assert.Equal(loanDate, loan.LoanDate);
        Assert.Equal(returnDate, loan.ReturnDate);
        Assert.Equal(StatusLoan.Returned, loan.Status);
    }

    [Fact]
    public void MarkAsReturned_OnActiveLoan_SetsReturnDateAndStatus()
    {
        var loan = Loan.Create(Guid.NewGuid());

        loan.MarkAsReturned();

        Assert.NotNull(loan.ReturnDate);
        Assert.True(DateTime.UtcNow - loan.ReturnDate! < Tolerance);
        Assert.Equal(StatusLoan.Returned, loan.Status);
    }

    [Fact]
    public void MarkAsReturned_OnActiveLoan_RaisesLoanReturnedEvent()
    {
        var loan = Loan.Create(Guid.NewGuid());
        loan.ClearDomainEvents();

        loan.MarkAsReturned();

        var domainEvent = Assert.Single(loan.DomainEvents);
        var loanReturnedEvent = Assert.IsType<LoanReturnedEvent>(domainEvent);
        Assert.Equal(loan.Id, loanReturnedEvent.LoanId);
        Assert.Equal(loan.ReturnDate, loanReturnedEvent.ReturnDate);
    }

    [Fact]
    public void MarkAsReturned_WhenAlreadyReturned_ThrowsDomainExceptionAndLeavesStateUnchanged()
    {
        var loan = Loan.Create(Guid.NewGuid());
        loan.MarkAsReturned();
        var returnDate = loan.ReturnDate;

        var exception = Assert.Throws<DomainException>(loan.MarkAsReturned);

        Assert.Equal("This loan has already been returned.", exception.Message);
        Assert.Equal(returnDate, loan.ReturnDate);
        Assert.Equal(StatusLoan.Returned, loan.Status);
    }

    [Fact]
    public void MarkAsReturned_WhenAlreadyReturned_RaisesNoAdditionalEvent()
    {
        var loan = Loan.Create(Guid.NewGuid());
        loan.MarkAsReturned();
        loan.ClearDomainEvents();

        Assert.Throws<DomainException>(loan.MarkAsReturned);

        Assert.Empty(loan.DomainEvents);
    }
}
