namespace OpeaLibrary.Domain.Events;

public sealed record BookQuantityChangedEvent(
    Guid BookId,
    int QuantityAvailable
) : IDomainEvent;
