namespace OpeaLibrary.Application.Loans.Commands.RequestLoan;

public sealed record RequestLoanCommand(Guid BookId) : IRequest<bool>;