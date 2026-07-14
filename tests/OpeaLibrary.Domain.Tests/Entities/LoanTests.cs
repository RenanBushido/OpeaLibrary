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
    public void Create_WithEmptyBookId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Loan.Create(Guid.Empty, Guid.NewGuid()));

        Assert.Equal("bookId", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptyUserId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => Loan.Create(Guid.NewGuid(), Guid.Empty));

        Assert.Equal("userId", exception.ParamName);
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
    public void MarkAsReturned_WhenAlreadyReturned_ThrowsInvalidOperationExceptionAndLeavesStateUnchanged()
    {
        var loan = Loan.Create(Guid.NewGuid(), Guid.NewGuid());
        loan.MarkAsReturned();
        var returnDate = loan.ReturnDate;

        Assert.Throws<InvalidOperationException>(loan.MarkAsReturned);

        Assert.Equal(returnDate, loan.ReturnDate);
        Assert.Equal(StatusLoan.Returned, loan.Status);
    }
}
