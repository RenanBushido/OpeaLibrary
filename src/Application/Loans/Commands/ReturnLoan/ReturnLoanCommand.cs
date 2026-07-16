namespace OpeaLibrary.Application.Loans.Commands.ReturnLoan;

public sealed record ReturnLoanCommand(Guid LoanId) : IRequest<bool>;