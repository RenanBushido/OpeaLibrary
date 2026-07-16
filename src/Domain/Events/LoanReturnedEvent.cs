namespace OpeaLibrary.Domain.Events;

public sealed record LoanReturnedEvent(
    Guid LoanId,
    DateTime ReturnDate
) : IDomainEvent;
