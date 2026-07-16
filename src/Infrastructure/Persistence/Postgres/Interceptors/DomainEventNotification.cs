namespace OpeaLibrary.Infrastructure.Persistence.Postgres.Interceptors;

public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;
