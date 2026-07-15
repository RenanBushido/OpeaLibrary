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
        var book = CreateAvailableBook(quantity: 2);
        await dbContext.Books.AddAsync(book);
        await dbContext.SaveChangesAsync();

        var repository = new LoanRepository(dbContext);
        var result = await repository.RequestLoanAsync(book.Id);
        await dbContext.SaveChangesAsync();

        Assert.True(result);

        var loans = await dbContext.Loans.ToListAsync();
        var loan = Assert.Single(loans);
        Assert.Equal(book.Id, loan.BookId);
        Assert.Equal(StatusLoan.Active, loan.Status);
        Assert.Null(loan.ReturnDate);

        var persistedBook = await dbContext.Books.FindAsync(book.Id);
        Assert.Equal(1, persistedBook!.QuantityAvailable);
    }

    [Fact]
    public async Task RequestLoanAsync_WhenBookHasNoAvailableCopies_ReturnsFalseAndPersistsNoLoan()
    {
        using var dbContext = TestDbContextFactory.Create();
        var book = CreateAvailableBook(quantity: 0);
        await dbContext.Books.AddAsync(book);
        await dbContext.SaveChangesAsync();

        var repository = new LoanRepository(dbContext);
        var result = await repository.RequestLoanAsync(book.Id);
        await dbContext.SaveChangesAsync();

        Assert.False(result);
        Assert.Empty(await dbContext.Loans.ToListAsync());
        Assert.Equal(0, book.QuantityAvailable);
    }

    [Fact]
    public async Task RequestLoanAsync_WhenBookDoesNotExist_ReturnsFalse()
    {
        using var dbContext = TestDbContextFactory.Create();
        var repository = new LoanRepository(dbContext);

        var result = await repository.RequestLoanAsync(Guid.NewGuid());

        Assert.False(result);
        Assert.Empty(await dbContext.Loans.ToListAsync());
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanIsActive_MarksReturnedAndIncrementsBookQuantity()
    {
        using var dbContext = TestDbContextFactory.Create();
        var book = CreateAvailableBook(quantity: 1);
        await dbContext.Books.AddAsync(book);
        await dbContext.SaveChangesAsync();

        var loanRepository = new LoanRepository(dbContext);
        await loanRepository.RequestLoanAsync(book.Id);
        await dbContext.SaveChangesAsync();
        var loan = Assert.Single(await dbContext.Loans.ToListAsync());

        var result = await loanRepository.ReturnLoanAsync(loan.Id);
        await dbContext.SaveChangesAsync();

        Assert.True(result);

        var persistedLoan = await dbContext.Loans.FindAsync(loan.Id);
        Assert.NotNull(persistedLoan!.ReturnDate);
        Assert.Equal(StatusLoan.Returned, persistedLoan.Status);

        var persistedBook = await dbContext.Books.FindAsync(book.Id);
        Assert.Equal(1, persistedBook!.QuantityAvailable);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenAssociatedBookNoLongerExists_StillMarksReturned()
    {
        using var dbContext = TestDbContextFactory.Create();
        var loan = Loan.Create(Guid.NewGuid());
        await dbContext.Loans.AddAsync(loan);
        await dbContext.SaveChangesAsync();

        var repository = new LoanRepository(dbContext);
        var result = await repository.ReturnLoanAsync(loan.Id);
        await dbContext.SaveChangesAsync();

        Assert.True(result);
        var persistedLoan = await dbContext.Loans.FindAsync(loan.Id);
        Assert.Equal(StatusLoan.Returned, persistedLoan!.Status);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanDoesNotExist_ReturnsFalse()
    {
        using var dbContext = TestDbContextFactory.Create();
        var repository = new LoanRepository(dbContext);

        var result = await repository.ReturnLoanAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanAlreadyReturned_ReturnsFalseAndDoesNotChangeQuantityAgain()
    {
        using var dbContext = TestDbContextFactory.Create();
        var book = CreateAvailableBook(quantity: 1);
        await dbContext.Books.AddAsync(book);
        await dbContext.SaveChangesAsync();

        var repository = new LoanRepository(dbContext);
        await repository.RequestLoanAsync(book.Id);
        await dbContext.SaveChangesAsync();
        var loan = Assert.Single(await dbContext.Loans.ToListAsync());

        await repository.ReturnLoanAsync(loan.Id);
        await dbContext.SaveChangesAsync();
        var quantityAfterFirstReturn = (await dbContext.Books.FindAsync(book.Id))!.QuantityAvailable;

        var result = await repository.ReturnLoanAsync(loan.Id);
        await dbContext.SaveChangesAsync();

        Assert.False(result);
        var quantityAfterSecondReturn = (await dbContext.Books.FindAsync(book.Id))!.QuantityAvailable;
        Assert.Equal(quantityAfterFirstReturn, quantityAfterSecondReturn);
    }

    [Fact]
    public async Task GetAllLoansAsync_WhenNoLoansPersisted_ReturnsEmptyCollection()
    {
        using var dbContext = TestDbContextFactory.Create();
        var repository = new LoanRepository(dbContext);

        var result = await repository.GetAllLoansAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllLoansAsync_WhenLoansPersisted_ReturnsAllLoans()
    {
        using var dbContext = TestDbContextFactory.Create();
        var loan1 = Loan.Create(Guid.NewGuid());
        var loan2 = Loan.Create(Guid.NewGuid());
        await dbContext.Loans.AddRangeAsync(loan1, loan2);
        await dbContext.SaveChangesAsync();

        var repository = new LoanRepository(dbContext);
        var result = await repository.GetAllLoansAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.Id == loan1.Id);
        Assert.Contains(result, l => l.Id == loan2.Id);
    }
}
