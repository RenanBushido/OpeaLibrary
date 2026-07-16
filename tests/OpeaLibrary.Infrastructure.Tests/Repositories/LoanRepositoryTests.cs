namespace OpeaLibrary.Infrastructure.Tests.Repositories;

public class LoanRepositoryTests
{
    private static Book CreateAvailableBook(int quantity = 1)
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);
        for (var i = 0; i < quantity; i++)
        {
            book.IncreaseQuantity();
        }

        return book;
    }

    [Fact]
    public async Task RequestLoanAsync_WhenBookIsAvailable_CreatesActiveLoanAndDecrementsQuantity()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var book = CreateAvailableBook(quantity: 2);
        await unitOfWork.BookRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        var result = await unitOfWork.LoanRepository.RequestLoanAsync(book.Id);
        await unitOfWork.CommitAsync();

        Assert.True(result);

        var loans = await dbContext.Loans.ToListAsync();
        var loan = Assert.Single(loans);
        Assert.Equal(book.Id, loan.BookId);
        Assert.Equal(StatusLoan.Active, loan.Status);
        Assert.Null(loan.ReturnDate);

        var persistedBook = await unitOfWork.BookRepository.GetBookByIdAsync(book.Id, CancellationToken.None);
        Assert.Equal(1, persistedBook!.QuantityAvailable);
    }

    [Fact]
    public async Task RequestLoanAsync_WhenBookHasNoAvailableCopies_ReturnsFalseAndPersistsNoLoan()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var book = CreateAvailableBook(quantity: 0);
        await unitOfWork.BookRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        var result = await unitOfWork.LoanRepository.RequestLoanAsync(book.Id);
        await unitOfWork.CommitAsync();

        Assert.False(result);
        Assert.Empty(await dbContext.Loans.ToListAsync());
        Assert.Equal(0, book.QuantityAvailable);
    }

    [Fact]
    public async Task RequestLoanAsync_WhenBookDoesNotExist_ReturnsFalse()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);

        var result = await unitOfWork.LoanRepository.RequestLoanAsync(Guid.NewGuid());

        Assert.False(result);
        Assert.Empty(await dbContext.Loans.ToListAsync());
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanIsActive_MarksReturnedAndIncrementsBookQuantity()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var book = CreateAvailableBook(quantity: 1);
        await unitOfWork.BookRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        await unitOfWork.LoanRepository.RequestLoanAsync(book.Id);
        await unitOfWork.CommitAsync();
        var loan = Assert.Single(await dbContext.Loans.ToListAsync());

        var result = await unitOfWork.LoanRepository.ReturnLoanAsync(loan.Id);
        await unitOfWork.CommitAsync();

        Assert.True(result);

        var persistedLoan = await dbContext.Loans.FindAsync(loan.Id);
        Assert.NotNull(persistedLoan!.ReturnDate);
        Assert.Equal(StatusLoan.Returned, persistedLoan.Status);

        var persistedBook = await unitOfWork.BookRepository.GetBookByIdAsync(book.Id, CancellationToken.None);
        Assert.Equal(1, persistedBook!.QuantityAvailable);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenAssociatedBookNoLongerExists_StillMarksReturned()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var loan = Loan.Create(Guid.NewGuid());
        // ILoanRepository has no method to insert an arbitrary, already-constructed Loan
        // (only RequestLoanAsync, which always creates its own Loan for an existing book),
        // so this orphaned-loan precondition must be seeded directly via the DbSet.
        await dbContext.Loans.AddAsync(loan);
        await unitOfWork.CommitAsync();

        var result = await unitOfWork.LoanRepository.ReturnLoanAsync(loan.Id);
        await unitOfWork.CommitAsync();

        Assert.True(result);
        var persistedLoan = await dbContext.Loans.FindAsync(loan.Id);
        Assert.Equal(StatusLoan.Returned, persistedLoan!.Status);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanDoesNotExist_ReturnsFalse()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);

        var result = await unitOfWork.LoanRepository.ReturnLoanAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanAlreadyReturned_ReturnsFalseAndDoesNotChangeQuantityAgain()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var book = CreateAvailableBook(quantity: 1);
        await unitOfWork.BookRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        await unitOfWork.LoanRepository.RequestLoanAsync(book.Id);
        await unitOfWork.CommitAsync();
        var loan = Assert.Single(await dbContext.Loans.ToListAsync());

        await unitOfWork.LoanRepository.ReturnLoanAsync(loan.Id);
        await unitOfWork.CommitAsync();
        var quantityAfterFirstReturn = (await unitOfWork.BookRepository.GetBookByIdAsync(book.Id, CancellationToken.None))!.QuantityAvailable;

        var result = await unitOfWork.LoanRepository.ReturnLoanAsync(loan.Id);
        await unitOfWork.CommitAsync();

        Assert.False(result);
        var quantityAfterSecondReturn = (await unitOfWork.BookRepository.GetBookByIdAsync(book.Id, CancellationToken.None))!.QuantityAvailable;
        Assert.Equal(quantityAfterFirstReturn, quantityAfterSecondReturn);
    }

    [Fact]
    public async Task GetAllLoansAsync_WhenNoLoansPersisted_ReturnsEmptyCollection()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);

        var result = await unitOfWork.LoanRepository.GetAllLoansAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllLoansAsync_WhenLoansPersisted_ReturnsAllLoans()
    {
        using var dbContext = TestDbContextFactory.Create();
        var unitOfWork = new UnitOfWork(dbContext);
        var loan1 = Loan.Create(Guid.NewGuid());
        var loan2 = Loan.Create(Guid.NewGuid());
        // Same limitation as above: ILoanRepository has no generic "add this loan" method,
        // so these freestanding loans (no associated book needed for this read-only test)
        // are seeded directly via the DbSet.
        await dbContext.Loans.AddRangeAsync(loan1, loan2);
        await unitOfWork.CommitAsync();

        var result = await unitOfWork.LoanRepository.GetAllLoansAsync(CancellationToken.None);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.Id == loan1.Id);
        Assert.Contains(result, l => l.Id == loan2.Id);
    }
}
