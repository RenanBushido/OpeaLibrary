# infrastructure-unit-tests Specification

## Purpose
Automated xUnit test suite validating the Infrastructure layer's Postgres persistence implementations (`BookRepository`, `LoanRepository`, `OpeaLibraryDbContext`, and EF Core entity configurations) against the Domain layer's business rules, using the EF Core InMemory provider so tests run without an external database. 100% line and branch coverage of `src/Infrastructure/Persistence/Postgres/` is the quality bar for this suite.

## Requirements

### Requirement: xUnit Test Project for Infrastructure Layer
The system SHALL provide an xUnit test project at `tests/OpeaLibrary.Infrastructure.Tests/` that references `OpeaLibrary.Infrastructure` (at `src/Infrastructure/OpeaLibrary.Infrastructure.csproj`) and is runnable via `dotnet test`, using the EF Core InMemory provider so no external Postgres database is required.

#### Scenario: Running the test suite
- **WHEN** a developer runs `dotnet test` against `tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj`
- **THEN** all Infrastructure unit tests execute and report pass/fail results using xUnit, with no external database connection required

### Requirement: Infrastructure Source Folder Correctly Named
The system SHALL locate the Infrastructure project source at `src/Infrastructure/` (not the previously misspelled `src/Infrastruture/`), with `src/OpeaLibrary.slnx` referencing the corrected path and folder label.

#### Scenario: Building the solution after the rename
- **WHEN** the solution is built via `src/OpeaLibrary.slnx`
- **THEN** the `OpeaLibrary.Infrastructure` project is found at `Infrastructure/OpeaLibrary.Infrastructure.csproj` and builds successfully

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

### Requirement: LoanRepository Adherence to Book Availability Invariant
The test suite SHALL verify that `LoanRepository.RequestLoanAsync` and `ReturnLoanAsync` enforce the Domain layer's `Book` availability invariant (`Book.DecreaseQuantity()` / `Book.IncreaseQuantity()`), not just create/update `Loan` records in isolation.

#### Scenario: Requesting a loan for an available book
- **WHEN** `RequestLoanAsync` is called with the `Id` of a persisted `Book` whose `QuantityAvailable` is greater than 0
- **THEN** the method returns `true`, a new `Loan` is persisted with `Status == StatusLoan.Active` and `ReturnDate == null`, and the book's `QuantityAvailable` is decremented by 1

#### Scenario: Requesting a loan for a book with no available copies
- **WHEN** `RequestLoanAsync` is called with the `Id` of a persisted `Book` whose `QuantityAvailable` is 0
- **THEN** the method returns `false`, no `Loan` is persisted, and the book's `QuantityAvailable` remains 0

#### Scenario: Requesting a loan for a book that does not exist
- **WHEN** `RequestLoanAsync` is called with a `Guid` that does not match any persisted book
- **THEN** the method returns `false` and no `Loan` is persisted

#### Scenario: Returning an active loan
- **WHEN** `ReturnLoanAsync` is called with the `Id` of a persisted `Loan` whose `ReturnDate` is null, and the loan's associated `Book` is persisted
- **THEN** the method returns `true`, the loan's `ReturnDate` is set and `Status == StatusLoan.Returned`, and the associated book's `QuantityAvailable` is incremented by 1

#### Scenario: Returning a loan that does not exist
- **WHEN** `ReturnLoanAsync` is called with a `Guid` that does not match any persisted loan
- **THEN** the method returns `false`

#### Scenario: Returning a loan that was already returned
- **WHEN** `ReturnLoanAsync` is called with the `Id` of a persisted `Loan` whose `ReturnDate` is already set
- **THEN** the method returns `false` and the associated book's `QuantityAvailable` is not changed again

#### Scenario: Getting all loans
- **WHEN** `GetAllLoansAsync` is called against a context with zero, one, or multiple persisted loans
- **THEN** it returns an empty collection, a single-item collection, or a multi-item collection matching what was persisted

### Requirement: EF Core Configuration Coverage
The test suite SHALL verify that `BookConfig` and `LoanConfig` are applied to `OpeaLibraryDbContext`'s model with the expected table names, keys, property facets, and relationships.

#### Scenario: Book entity configuration is applied
- **WHEN** `OpeaLibraryDbContext.Model` is inspected for the `Book` entity type
- **THEN** it maps to table `tb_books`, has `Id` as its key, and marks `Title` (max length 200), `Author` (max length 100), `PublishedYear`, and `QuantityAvailable` as required

#### Scenario: Loan entity configuration is applied
- **WHEN** `OpeaLibraryDbContext.Model` is inspected for the `Loan` entity type
- **THEN** it maps to table `tb_loans`, has `Id` as its key, marks `BookId` and `LoanDate` as required, and marks `ReturnDate` as optional

#### Scenario: Loan entity has a foreign key relationship to Book
- **WHEN** `OpeaLibraryDbContext.Model` is inspected for the `Loan` entity type's foreign keys
- **THEN** it has exactly one foreign key on `BookId` referencing `Book`, with `DeleteBehavior.Restrict`

### Requirement: 100% Code Coverage of Infrastructure Layer
The test suite SHALL achieve 100% line and branch coverage of all types under `src/Infrastructure/Persistence/Postgres/`, verifiable via `dotnet test --collect:"XPlat Code Coverage"`.

#### Scenario: Verifying coverage after running tests
- **WHEN** `dotnet test --collect:"XPlat Code Coverage"` is run against the Infrastructure test project
- **THEN** the generated Cobertura coverage report shows 100% line coverage and 100% branch coverage for `src/Infrastructure/Persistence/Postgres/**/*.cs`

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
