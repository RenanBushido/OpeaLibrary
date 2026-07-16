namespace OpeaLibrary.Application.Loans.Commands.ReturnLoan;

public sealed class ReturnLoanValidator : AbstractValidator<ReturnLoanCommand>
{
    public ReturnLoanValidator()
    {
        RuleFor(x => x.LoanId)
            .NotEmpty().WithMessage("LoanId is required.")
            .Must(id => id != Guid.Empty).WithMessage("The Loan Id must be a valid GUID.");
    }
}