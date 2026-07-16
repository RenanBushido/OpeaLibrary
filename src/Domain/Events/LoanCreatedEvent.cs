namespace OpeaLibrary.Domain.Events;

public sealed record LoanCreatedEvent(
    Guid LoanId,
    Guid BookId,
    DateTime LoanDate
) : IDomainEvent;
