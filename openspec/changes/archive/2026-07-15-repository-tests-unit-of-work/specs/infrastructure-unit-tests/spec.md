## MODIFIED Requirements

### Requirement: BookRepository Coverage
The test suite SHALL exercise every code path of `BookRepository.AddBookAsync`, `GetBookByIdAsync`, and `GetAllBooksAsync` against an EF Core InMemory-backed `OpeaLibraryDbContext`, persisting changes through an `IUnitOfWork` instance rather than calling `DbContext.SaveChangesAsync()` directly.

#### Scenario: Adding a book
- **WHEN** `AddBookAsync` is called with a valid `Book` and the change is committed via `IUnitOfWork.CommitAsync()`
- **THEN** the book becomes retrievable from the `DbContext`

#### Scenario: Getting a book by id that exists
- **WHEN** `GetBookByIdAsync` is called with the `Id` of a book already persisted in the context
- **THEN** the matching `Book` is returned

#### Scenario: Getting a book by id that does not exist
- **WHEN** `GetBookByIdAsync` is called with a `Guid` that does not match any persisted book
- **THEN** `null` is returned

#### Scenario: Getting all books
- **WHEN** `GetAllBooksAsync` is called against a context with zero, one, or multiple persisted books
- **THEN** it returns an empty collection, a single-item collection, or a multi-item collection matching what was persisted

## ADDED Requirements

### Requirement: IUnitOfWork Implementation for Postgres Persistence
The system SHALL provide a concrete `UnitOfWork` class implementing `IUnitOfWork`, exposing `BookRepository` and `LoanRepository` backed by the same `OpeaLibraryDbContext` instance, and persisting all staged changes via `CommitAsync`.

#### Scenario: Committing staged changes
- **WHEN** `CommitAsync` is called on a `UnitOfWork` after repository methods (e.g. `AddBookAsync`, `RequestLoanAsync`) have staged changes
- **THEN** the staged changes are persisted to the underlying `DbContext`

#### Scenario: Repositories share the same DbContext instance
- **WHEN** `BookRepository` and `LoanRepository` are accessed from the same `UnitOfWork` instance
- **THEN** both repositories operate against the same `OpeaLibraryDbContext`, so a change staged through one is visible to a query made through the other before `CommitAsync` is called

### Requirement: Infrastructure Tests Persist Through IUnitOfWork
The test suite SHALL exercise repository behavior the way production code is intended to use it: acquiring `BookRepository`/`LoanRepository` from an `IUnitOfWork` instance and persisting changes via `IUnitOfWork.CommitAsync()`, not by calling `DbContext.SaveChangesAsync()` directly in the "act" step of a test.

#### Scenario: Test commits via UnitOfWork
- **WHEN** a repository test exercises `AddBookAsync`, `RequestLoanAsync`, or `ReturnLoanAsync`
- **THEN** the test commits the resulting change via `unitOfWork.CommitAsync()` rather than `dbContext.SaveChangesAsync()`

#### Scenario: Seeding a precondition the repository API cannot express
- **WHEN** a test needs to seed a `Loan` that no `ILoanRepository` method can construct directly (e.g. an orphaned loan with no associated book, or a freestanding loan seeded for a read-only assertion)
- **THEN** the test may add the entity directly via the `DbContext`'s `DbSet`, but still commits it via `unitOfWork.CommitAsync()` rather than `dbContext.SaveChangesAsync()`
