namespace OpeaLibrary.Presentation.Tests.Endpoints;

public class LoanEndpointTests
{
    private static bool GetResultProperty(IResult apiResult)
    {
        var value = Assert.IsAssignableFrom<IValueHttpResult>(apiResult).Value!;
        var property = value.GetType().GetProperty("result")
            ?? throw new InvalidOperationException("Expected an anonymous object with a 'result' property.");

        return (bool)property.GetValue(value)!;
    }

    [Fact]
    public async Task RequestLoan_SendsRequestLoanCommandWithRouteIdAsBookId()
    {
        var mediator = new Mock<IMediator>();
        var bookId = Guid.NewGuid();
        mediator
            .Setup(m => m.Send(It.IsAny<RequestLoanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await LoanEndpoint.RequestLoan(bookId, mediator.Object);

        mediator.Verify(
            m => m.Send(It.Is<RequestLoanCommand>(c => c.BookId == bookId), It.IsAny<CancellationToken>()),
            Times.Once);

        var statusCodeResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);

        Assert.True(GetResultProperty(result));
    }

    [Fact]
    public async Task RequestLoan_WhenCommandFails_StillReturnsOkWithFalseResult()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<RequestLoanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await LoanEndpoint.RequestLoan(Guid.NewGuid(), mediator.Object);

        Assert.False(GetResultProperty(result));
    }

    [Fact]
    public async Task ReturnLoan_SendsReturnLoanCommandWithRouteIdAsLoanId()
    {
        var mediator = new Mock<IMediator>();
        var loanId = Guid.NewGuid();
        mediator
            .Setup(m => m.Send(It.IsAny<ReturnLoanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await LoanEndpoint.ReturnLoan(loanId, mediator.Object);

        mediator.Verify(
            m => m.Send(It.Is<ReturnLoanCommand>(c => c.LoanId == loanId), It.IsAny<CancellationToken>()),
            Times.Once);

        var statusCodeResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status200OK, statusCodeResult.StatusCode);

        Assert.True(GetResultProperty(result));
    }

    [Fact]
    public async Task ReturnLoan_WhenCommandFails_StillReturnsOkWithFalseResult()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<ReturnLoanCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await LoanEndpoint.ReturnLoan(Guid.NewGuid(), mediator.Object);

        Assert.False(GetResultProperty(result));
    }

    [Fact]
    public async Task GetAllLoans_SendsGetAllLoansRequest_ReturnsMappedResponseForEachLoan()
    {
        var mediator = new Mock<IMediator>();
        var loan = new GetAllLoansResponse(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, null, StatusLoan.Active);
        mediator
            .Setup(m => m.Send(It.IsAny<GetAllLoansRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([loan]);

        var result = await LoanEndpoint.GetAllLoans(mediator.Object, CancellationToken.None);

        mediator.Verify(m => m.Send(It.IsAny<GetAllLoansRequest>(), It.IsAny<CancellationToken>()), Times.Once);

        var okResult = Assert.IsType<Ok<List<LoanEndpoint.LoanResponse>>>(result);
        var response = Assert.Single(okResult.Value!);
        Assert.Equal(loan.Id, response.Id);
        Assert.Equal(loan.BookId, response.BookId);
        Assert.Equal(loan.LoanDate, response.LoanDate);
        Assert.Equal(loan.ReturnDate, response.ReturnDate);
        Assert.Equal(loan.Status, response.Status);
    }

    [Fact]
    public async Task GetAllLoans_WhenNoLoansExist_ReturnsEmptyList()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<GetAllLoansRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await LoanEndpoint.GetAllLoans(mediator.Object, CancellationToken.None);

        var okResult = Assert.IsType<Ok<List<LoanEndpoint.LoanResponse>>>(result);
        Assert.Empty(okResult.Value!);
    }
}
