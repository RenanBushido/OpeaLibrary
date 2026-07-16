## ADDED Requirements

### Requirement: DomainEventsInterceptor Dispatches Only After a Successful Save
The test suite SHALL verify that `DomainEventsInterceptor` publishes an entity's domain events via `IMediator` after `SaveChangesAsync` succeeds, clears those events off the entity afterward, and does not publish anything when `SaveChangesAsync` fails.

#### Scenario: Successful save dispatches and clears events
- **WHEN** `SaveChangesAsync` is called on an `OpeaLibraryDbContext` with a tracked entity that has one or more `DomainEvents`, and the save succeeds
- **THEN** each event is published via `IMediator.Publish` (wrapped as a notification), and the entity's `DomainEvents` is empty afterward

#### Scenario: Failed save dispatches nothing
- **WHEN** `SaveChangesAsync` fails (e.g. a constraint violation)
- **THEN** `IMediator.Publish` is never called for that entity's pending domain events

### Requirement: Mongo Projection Event Handlers Coverage
The test suite SHALL exercise each MongoDB projection notification handler (`BookCreatedEventHandler`, `BookQuantityChangedEventHandler`, `LoanCreatedEventHandler`, `LoanReturnedEventHandler`) against an ephemeral, in-process MongoDB instance, asserting the resulting `books`/`loans` document state.

#### Scenario: BookCreatedEventHandler upserts a new book document
- **WHEN** `BookCreatedEventHandler` handles a `BookCreatedEvent`
- **THEN** a `books` document with the matching `Id`, `Title`, `Author`, `PublishedYear`, and `QuantityAvailable` exists afterward

#### Scenario: BookQuantityChangedEventHandler updates the book's quantity
- **WHEN** `BookQuantityChangedEventHandler` handles a `BookQuantityChangedEvent` for a book document that already exists
- **THEN** that document's `QuantityAvailable` field matches the event's value, with no other fields changed

#### Scenario: LoanCreatedEventHandler upserts a new loan document
- **WHEN** `LoanCreatedEventHandler` handles a `LoanCreatedEvent`
- **THEN** a `loans` document with the matching `Id`, `BookId`, `LoanDate`, `ReturnDate` (null), and `Status` (`Active`) exists afterward

#### Scenario: LoanReturnedEventHandler updates the loan's return state
- **WHEN** `LoanReturnedEventHandler` handles a `LoanReturnedEvent` for a loan document that already exists
- **THEN** that document's `ReturnDate` and `Status` fields reflect the return, with no other fields changed

### Requirement: Projection Handler Failures Are Logged, Not Propagated
Each Mongo projection notification handler SHALL catch its own exceptions and log them rather than letting them propagate out of `IMediator.Publish`, so a projection failure does not surface as a failure of the originating command.

#### Scenario: A projection write throws
- **WHEN** a projection notification handler's MongoDB write throws (e.g. the connection fails)
- **THEN** the handler catches the exception, logs it, and returns normally rather than throwing
