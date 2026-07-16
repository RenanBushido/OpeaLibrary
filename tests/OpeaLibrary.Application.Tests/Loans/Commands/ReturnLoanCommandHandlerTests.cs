namespace OpeaLibrary.Application.Tests.Loans.Commands;

public class ReturnLoanCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRepositorySucceeds_ReturnsTrueAndCommits()
    {
        var loanRepository = new Mock<ILoanRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var loanId = Guid.NewGuid();

        loanRepository.Setup(r => r.ReturnLoanAsync(loanId)).ReturnsAsync(true);
        unitOfWork.Setup(u => u.LoanRepository).Returns(loanRepository.Object);

        var handler = new ReturnLoanCommandHandler(unitOfWork.Object);

        var result = await handler.Handle(new ReturnLoanCommand(loanId), CancellationToken.None);

        Assert.True(result);
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ReturnsFalseAndDoesNotCommit()
    {
        var loanRepository = new Mock<ILoanRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var loanId = Guid.NewGuid();

        loanRepository.Setup(r => r.ReturnLoanAsync(loanId)).ReturnsAsync(false);
        unitOfWork.Setup(u => u.LoanRepository).Returns(loanRepository.Object);

        var handler = new ReturnLoanCommandHandler(unitOfWork.Object);

        var result = await handler.Handle(new ReturnLoanCommand(loanId), CancellationToken.None);

        Assert.False(result);
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
