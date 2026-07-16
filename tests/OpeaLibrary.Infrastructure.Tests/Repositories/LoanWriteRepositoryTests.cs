namespace OpeaLibrary.Infrastructure.Tests.Repositories;

public class LoanWriteRepositoryTests
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
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;
        var unitOfWork = new UnitOfWork(dbContext);
        var book = CreateAvailableBook(quantity: 2);
        await unitOfWork.BookWriteRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        var result = await unitOfWork.LoanWriteRepository.RequestLoanAsync(book.Id);
        await unitOfWork.CommitAsync();

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
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;
        var unitOfWork = new UnitOfWork(dbContext);
        var book = CreateAvailableBook(quantity: 0);
        await unitOfWork.BookWriteRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        var result = await unitOfWork.LoanWriteRepository.RequestLoanAsync(book.Id);
        await unitOfWork.CommitAsync();

        Assert.False(result);
        Assert.Empty(await dbContext.Loans.ToListAsync());
        Assert.Equal(0, book.QuantityAvailable);
    }

    [Fact]
    public async Task RequestLoanAsync_WhenBookDoesNotExist_ReturnsFalse()
    {
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;
        var unitOfWork = new UnitOfWork(dbContext);

        var result = await unitOfWork.LoanWriteRepository.RequestLoanAsync(Guid.NewGuid());

        Assert.False(result);
        Assert.Empty(await dbContext.Loans.ToListAsync());
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanIsActive_MarksReturnedAndIncrementsBookQuantity()
    {
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;
        var unitOfWork = new UnitOfWork(dbContext);
        var book = CreateAvailableBook(quantity: 1);
        await unitOfWork.BookWriteRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        await unitOfWork.LoanWriteRepository.RequestLoanAsync(book.Id);
        await unitOfWork.CommitAsync();
        var loan = Assert.Single(await dbContext.Loans.ToListAsync());

        var result = await unitOfWork.LoanWriteRepository.ReturnLoanAsync(loan.Id);
        await unitOfWork.CommitAsync();

        Assert.True(result);

        var persistedLoan = await dbContext.Loans.FindAsync(loan.Id);
        Assert.NotNull(persistedLoan!.ReturnDate);
        Assert.Equal(StatusLoan.Returned, persistedLoan.Status);

        var persistedBook = await dbContext.Books.FindAsync(book.Id);
        Assert.Equal(1, persistedBook!.QuantityAvailable);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanDoesNotExist_ReturnsFalse()
    {
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;
        var unitOfWork = new UnitOfWork(dbContext);

        var result = await unitOfWork.LoanWriteRepository.ReturnLoanAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task ReturnLoanAsync_WhenLoanAlreadyReturned_ReturnsFalseAndDoesNotChangeQuantityAgain()
    {
        using var database = TestDbContextFactory.Create();
        var dbContext = database.Context;
        var unitOfWork = new UnitOfWork(dbContext);
        var book = CreateAvailableBook(quantity: 1);
        await unitOfWork.BookWriteRepository.AddBookAsync(book);
        await unitOfWork.CommitAsync();

        await unitOfWork.LoanWriteRepository.RequestLoanAsync(book.Id);
        await unitOfWork.CommitAsync();
        var loan = Assert.Single(await dbContext.Loans.ToListAsync());

        await unitOfWork.LoanWriteRepository.ReturnLoanAsync(loan.Id);
        await unitOfWork.CommitAsync();
        var quantityAfterFirstReturn = (await dbContext.Books.FindAsync(book.Id))!.QuantityAvailable;

        var result = await unitOfWork.LoanWriteRepository.ReturnLoanAsync(loan.Id);
        await unitOfWork.CommitAsync();

        Assert.False(result);
        var quantityAfterSecondReturn = (await dbContext.Books.FindAsync(book.Id))!.QuantityAvailable;
        Assert.Equal(quantityAfterFirstReturn, quantityAfterSecondReturn);
    }
}
