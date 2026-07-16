namespace OpeaLibrary.Application.Tests.Loans.Commands;

public class RequestLoanCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRepositorySucceeds_ReturnsTrueAndCommits()
    {
        var loanRepository = new Mock<ILoanWriteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var bookId = Guid.NewGuid();

        loanRepository.Setup(r => r.RequestLoanAsync(bookId)).ReturnsAsync(true);
        unitOfWork.Setup(u => u.LoanWriteRepository).Returns(loanRepository.Object);

        var handler = new RequestLoanCommandHandler(unitOfWork.Object);

        var result = await handler.Handle(new RequestLoanCommand(bookId), CancellationToken.None);

        Assert.True(result);
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ReturnsFalseAndDoesNotCommit()
    {
        var loanRepository = new Mock<ILoanWriteRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var bookId = Guid.NewGuid();

        loanRepository.Setup(r => r.RequestLoanAsync(bookId)).ReturnsAsync(false);
        unitOfWork.Setup(u => u.LoanWriteRepository).Returns(loanRepository.Object);

        var handler = new RequestLoanCommandHandler(unitOfWork.Object);

        var result = await handler.Handle(new RequestLoanCommand(bookId), CancellationToken.None);

        Assert.False(result);
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
