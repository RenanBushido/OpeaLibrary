# cqrs-read-model-sync Specification

## Purpose
TBD - created by syncing change sync-cqrs-read-write. Update Purpose after archive.

## Requirements

### Requirement: Book Write Operations Propagate to the Mongo Read Side
The system SHALL ensure that every successful `AddBookAsync`, `RequestLoanAsync` (book quantity decrement), and `ReturnLoanAsync` (book quantity increment) results in the corresponding `books` MongoDB document reflecting the new state, without requiring any change to the Application-layer command handlers.

#### Scenario: A newly added book becomes visible on the read side
- **WHEN** `AddBookCommandHandler` successfully commits a new `Book`
- **THEN** a matching document appears in the `books` MongoDB collection with the same `Id`, `Title`, `Author`, `PublishedYear`, and `QuantityAvailable`, retrievable via `GetBookByIdAsync`/`GetAllBooksAsync`

#### Scenario: A book's quantity change becomes visible on the read side
- **WHEN** `RequestLoanCommandHandler` or `ReturnLoanCommandHandler` successfully commits a change to a `Book`'s `QuantityAvailable`
- **THEN** the corresponding `books` MongoDB document's `QuantityAvailable` field reflects the new value

### Requirement: Loan Write Operations Propagate to the Mongo Read Side
The system SHALL ensure that every successful `RequestLoanAsync` (loan creation) and `ReturnLoanAsync` (loan return) results in the corresponding `loans` MongoDB document reflecting the new state, without requiring any change to the Application-layer command handlers.

#### Scenario: A newly requested loan becomes visible on the read side
- **WHEN** `RequestLoanCommandHandler` successfully commits a new `Loan`
- **THEN** a matching document appears in the `loans` MongoDB collection with the same `Id`, `BookId`, `LoanDate`, `ReturnDate` (null), and `Status` (`Active`), retrievable via `GetAllLoansAsync`

#### Scenario: A returned loan becomes visible on the read side
- **WHEN** `ReturnLoanCommandHandler` successfully commits a loan return
- **THEN** the corresponding `loans` MongoDB document's `ReturnDate` and `Status` fields reflect the return

### Requirement: Domain Events Are the Propagation Mechanism
`Book` and `Loan` SHALL raise domain events as a side effect of their state-changing methods (`Create`, `DecreaseQuantity`, `IncreaseQuantity`, `MarkAsReturned`); these events SHALL be dispatched via MediatR only after the originating `SaveChangesAsync` call succeeds, and never when it fails.

#### Scenario: A failed commit raises no dispatched events
- **WHEN** a write operation's `SaveChangesAsync` call fails (e.g. a constraint violation)
- **THEN** no domain events raised by the entities in that failed operation are dispatched, and the Mongo read side is not modified

#### Scenario: Dispatched events do not leak across unrelated commits
- **WHEN** an entity's domain events are dispatched after a successful `SaveChangesAsync`
- **THEN** those events are cleared from the entity afterward, so a later, unrelated `SaveChangesAsync` involving the same tracked entity instance does not re-dispatch them

### Requirement: A Projection Failure Does Not Misreport a Successful Write
A failure while writing a MongoDB projection SHALL NOT cause the originating command to report failure to its caller when the underlying Postgres write already succeeded; it SHALL be logged instead.

#### Scenario: Mongo is unreachable during projection
- **WHEN** a command's Postgres write succeeds but the subsequent Mongo projection write throws (e.g. MongoDB is unreachable)
- **THEN** the command still reports success to its caller (matching what actually happened in Postgres), and the projection failure is logged rather than propagated as the command's result
