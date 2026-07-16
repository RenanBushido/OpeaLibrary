namespace OpeaLibrary.Application.Loans.Queries.GetAllLoans;

public sealed record GetAllLoansResponse(
    Guid Id,
    Guid BookId,
    DateTime LoanDate,
    DateTime? ReturnDate,
    StatusLoan Status
);