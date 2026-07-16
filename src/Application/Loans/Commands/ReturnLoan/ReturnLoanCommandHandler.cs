namespace OpeaLibrary.Application.Loans.Commands.ReturnLoan;

public sealed class ReturnLoanCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ReturnLoanCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> Handle(ReturnLoanCommand request, CancellationToken cancellationToken)
    {
        var succeeded = await _unitOfWork.LoanWriteRepository.ReturnLoanAsync(request.LoanId);

        if (!succeeded) return false;

        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}