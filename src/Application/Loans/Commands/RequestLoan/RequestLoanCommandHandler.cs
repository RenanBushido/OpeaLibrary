namespace OpeaLibrary.Application.Loans.Commands.RequestLoan;

public sealed class RequestLoanCommandHandler(    
    IUnitOfWork unitOfWork
) : IRequestHandler<RequestLoanCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<bool> Handle(RequestLoanCommand request, CancellationToken cancellationToken)
    {
        var succeeded = await _unitOfWork.LoanWriteRepository.RequestLoanAsync(request.BookId);

        if (!succeeded) return false;

        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}