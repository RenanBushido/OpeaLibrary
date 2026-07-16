namespace OpeaLibrary.Domain.Events;

public sealed record BookCreatedEvent(
    Guid BookId,
    string Title,
    string Author,
    int PublishedYear,
    int QuantityAvailable
) : IDomainEvent;
