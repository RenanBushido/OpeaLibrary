namespace OpeaLibrary.Application.Tests.Loans.Queries;

public class GetAllLoansQueryHandlerTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    [Fact]
    public async Task Handle_ReturnsMappedResponseForEachLoan()
    {
        var loan = Loan.Create(Guid.NewGuid());
        var loanRepository = new Mock<ILoanReadRepository>();
        loanRepository.Setup(r => r.GetAllLoansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([loan]);

        var handler = new GetAllLoansQueryHandler(loanRepository.Object, CreateMapper());

        var result = await handler.Handle(new GetAllLoansRequest(), CancellationToken.None);

        var response = Assert.Single(result);
        Assert.Equal(loan.Id, response.Id);
        Assert.Equal(loan.BookId, response.BookId);
        Assert.Equal(loan.LoanDate, response.LoanDate);
        Assert.Equal(loan.ReturnDate, response.ReturnDate);
        Assert.Equal(loan.Status, response.Status);
    }
}
