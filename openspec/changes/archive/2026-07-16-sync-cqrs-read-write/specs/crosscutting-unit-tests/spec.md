## MODIFIED Requirements

### Requirement: ApplicationExtensions Registration Coverage
The test suite SHALL verify that `ApplicationExtensions.AddApiMediatR`, `AddApiAutoMapper`, and `AddApiValidators` each register their expected services on an `IServiceCollection`, and that `AddApiMediatR`'s MediatR assembly scan discovers notification handlers defined in `OpeaLibrary.Infrastructure`, not just request handlers in `OpeaLibrary.Application`.

#### Scenario: AddApiMediatR registers MediatR handlers and the validation pipeline behavior
- **WHEN** `AddApiMediatR` is called on an `IServiceCollection`
- **THEN** the resulting `IServiceCollection` contains registrations resolving `IMediator`, and includes `ValidationBehavior<,>` as a registered open pipeline behavior

#### Scenario: AddApiMediatR discovers Infrastructure notification handlers
- **WHEN** `AddApiMediatR` is called on an `IServiceCollection`
- **THEN** the resulting `IServiceCollection` contains registrations resolving `INotificationHandler<DomainEventNotification<BookCreatedEvent>>` to `BookCreatedEventHandler`, `INotificationHandler<DomainEventNotification<BookQuantityChangedEvent>>` to `BookQuantityChangedEventHandler`, `INotificationHandler<DomainEventNotification<LoanCreatedEvent>>` to `LoanCreatedEventHandler`, and `INotificationHandler<DomainEventNotification<LoanReturnedEvent>>` to `LoanReturnedEventHandler`

#### Scenario: AddApiAutoMapper registers the MappingProfile
- **WHEN** `AddApiAutoMapper` is called on an `IServiceCollection` and the collection is built into a `ServiceProvider`
- **THEN** an `IMapper` can be resolved and its configuration includes the maps declared in `MappingProfile`

#### Scenario: AddApiValidators registers all four validators
- **WHEN** `AddApiValidators` is called on an `IServiceCollection` and the collection is built into a `ServiceProvider`
- **THEN** `IValidator<AddBookCommand>`, `IValidator<GetBookByIdRequest>`, `IValidator<RequestLoanCommand>`, and `IValidator<ReturnLoanCommand>` each resolve to their expected validator implementation with a scoped lifetime
