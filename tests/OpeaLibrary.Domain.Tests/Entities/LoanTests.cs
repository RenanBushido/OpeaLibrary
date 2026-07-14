namespace OpeaLibrary.Domain.Tests.Entities;

public class LoanTests
{
    private static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(5);

    [Fact]
    public void Create_WithValidData_ReturnsLoanWithExpectedValues()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var loan = Loan.Create(bookId, userId);

        Assert.NotEqual(Guid.Empty, loan.Id);
        Assert.Equal(bookId, loan.BookId);
        Assert.Equal(userId, loan.UserId);
        Assert.True(DateTime.UtcNow - loan.LoanDate < Tolerance);
        Assert.Null(loan.ReturnDate);
        Assert.Equal(StatusLoan.Active, loan.Status);
    }

    [Fact]
    public void Create_WithEmptyBookId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() => Loan.Create(Guid.Empty, Guid.NewGuid()));

        Assert.Equal("Book ID cannot be empty.", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() => Loan.Create(Guid.NewGuid(), Guid.Empty));

        Assert.Equal("User ID cannot be empty.", exception.Message);
    }

    [Fact]
    public void MarkAsReturned_OnActiveLoan_SetsReturnDateAndStatus()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid());

        loan.MarkAsReturned();

        Assert.NotNull(loan.ReturnDate);
        Assert.True(DateTime.UtcNow - loan.ReturnDate! < Tolerance);
        Assert.Equal(StatusLoan.Returned, loan.Status);
    }

    [Fact]
    public void MarkAsReturned_WhenAlreadyReturned_ThrowsDomainExceptionAndLeavesStateUnchanged()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid());
        loan.MarkAsReturned();
        var returnDate = loan.ReturnDate;

        var exception = Assert.Throws<DomainException>(loan.MarkAsReturned);

        Assert.Equal("This loan has already been returned.", exception.Message);
        Assert.Equal(returnDate, loan.ReturnDate);
        Assert.Equal(StatusLoan.Returned, loan.Status);
    }
}
