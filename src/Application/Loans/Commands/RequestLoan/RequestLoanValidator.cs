namespace OpeaLibrary.Application.Loans.Commands.RequestLoan;

public sealed class RequestLoanValidator : AbstractValidator<RequestLoanCommand>
{
    public RequestLoanValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("The book Id must not be empty.")
            .Must(id => id != Guid.Empty).WithMessage("The book Id must be a valid GUID.");
    }
}