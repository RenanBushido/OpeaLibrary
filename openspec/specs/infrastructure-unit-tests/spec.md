# infrastructure-unit-tests Specification

## Purpose
Automated xUnit test suite validating the Infrastructure layer's persistence implementations for both the Postgres write side (`BookWriteRepository`, `LoanWriteRepository`, `OpeaLibraryDbContext`, and EF Core entity configurations) and the MongoDB read side (`BookReadRepository`, `LoanReadRepository`) against the Domain layer's business rules, using a SQLite in-memory database for Postgres write-repository tests and an ephemeral, in-process MongoDB instance for Mongo read-repository tests, so no external Postgres or MongoDB server is required. 100% line and branch coverage of `src/Infrastructure/Persistence/Postgres/` and `src/Infrastructure/Persistence/Mongodb/` is the quality bar for this suite, with two documented exceptions (see the 100% Code Coverage requirement).

## Requirements

### Requirement: xUnit Test Project for Infrastructure Layer
The system SHALL provide an xUnit test project at `tests/OpeaLibrary.Infrastructure.Tests/` that references `OpeaLibrary.Infrastructure` (at `src/Infrastructure/OpeaLibrary.Infrastructure.csproj`) and is runnable via `dotnet test`, using a SQLite in-memory database for Postgres write-repository tests and an ephemeral, in-process MongoDB instance for Mongo read-repository tests, so no external Postgres or MongoDB server is required.

#### Scenario: Running the test suite
- **WHEN** a developer runs `dotnet test` against `tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj`
- **THEN** all Infrastructure unit tests execute and report pass/fail results using xUnit, with no external database connection required

### Requirement: Infrastructure Source Folder Correctly Named
The system SHALL locate the Infrastructure project source at `src/Infrastructure/` (not the previously misspelled `src/Infrastruture/`), with `src/OpeaLibrary.slnx` referencing the corrected path and folder label.

#### Scenario: Building the solution after the rename
- **WHEN** the solution is built via `src/OpeaLibrary.slnx`
- **THEN** the `OpeaLibrary.Infrastructure` project is found at `Infrastructure/OpeaLibrary.Infrastructure.csproj` and builds successfully

### Requirement: BookWriteRepository Coverage
The test suite SHALL exercise every code path of `BookWriteRepository.AddBookAsync` against a SQLite in-memory-backed `OpeaLibraryDbContext`, persisting changes through an `IUnitOfWork` instance rather than calling `DbContext.SaveChangesAsync()` directly, and asserting the persisted result via direct `DbContext` queries (`BookWriteRepository` has no read methods of its own).

#### Scenario: Adding a book
- **WHEN** `AddBookAsync` is called with a valid `Book` and the change is committed via `IUnitOfWork.CommitAsync()`
- **THEN** the book becomes retrievable via a direct query against the `DbContext`'s `Books` set

### Requirement: LoanWriteRepository Adherence to Book Availability Invariant
The test suite SHALL verify that `LoanWriteRepository.RequestLoanAsync` and `ReturnLoanAsync` enforce the Domain layer's `Book` availability invariant (`Book.DecreaseQuantity()` / `Book.IncreaseQuantity()`), not just create/update `Loan` records in isolation, against a SQLite in-memory-backed `OpeaLibraryDbContext`, asserting persisted results via direct `DbContext` queries.

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
The test suite SHALL achieve 100% line and branch coverage of all types under `src/Infrastructure/Persistence/Postgres/` and `src/Infrastructure/Persistence/Mongodb/`, verifiable via `dotnet test --collect:"XPlat Code Coverage"`, with two documented exceptions that are unreachable or unused through no gap in test design:
- `LoanWriteRepository.ReturnLoanAsync`'s `book?.IncreaseQuantity()` null-conditional branch (the case where the loan's associated book no longer exists) is excluded. The `Loan.BookId → Book` foreign key (`DeleteBehavior.Restrict`) guarantees a loan's book always exists, making this branch unreachable under both the SQLite in-memory test provider and real Postgres alike; the previous spec's InMemory-provider-only "orphaned loan" scenario tested a state the FK doesn't actually allow. Closing this gap would mean modifying `src/` production code (removing the now-dead defensive null-conditional), which is out of scope for this test-alignment change.
- `MongoMappings.Configure()` (`src/Infrastructure/Persistence/Mongodb/Mappings/MongoMapping.cs`) is excluded. It is not called by any code path in `src/` — `InfrastructureExtensions.AddInfraMongo` registers the `GuidSerializer` directly and never invokes it — so it is untested dead/orphaned code, not a test gap. Deciding its fate (wire it in, or delete it) is a `src/` production-code decision out of scope for this change.

#### Scenario: Verifying coverage after running tests
- **WHEN** `dotnet test --collect:"XPlat Code Coverage"` is run against the Infrastructure test project
- **THEN** the generated Cobertura coverage report shows 100% line coverage and 100% branch coverage for `src/Infrastructure/Persistence/Postgres/**/*.cs` and `src/Infrastructure/Persistence/Mongodb/**/*.cs`, except for the two documented exceptions above

### Requirement: IUnitOfWork Implementation for Postgres Persistence
The system SHALL provide a concrete `UnitOfWork` class implementing `IUnitOfWork`, exposing `BookWriteRepository` and `LoanWriteRepository` backed by the same `OpeaLibraryDbContext` instance, and persisting all staged changes via `CommitAsync`.

#### Scenario: Committing staged changes
- **WHEN** `CommitAsync` is called on a `UnitOfWork` after repository methods (e.g. `AddBookAsync`, `RequestLoanAsync`) have staged changes
- **THEN** the staged changes are persisted to the underlying `DbContext`

#### Scenario: Repositories share the same DbContext instance
- **WHEN** `BookWriteRepository` and `LoanWriteRepository` are accessed from the same `UnitOfWork` instance
- **THEN** both repositories operate against the same `OpeaLibraryDbContext`, so a change staged through one is visible to a query made through the other before `CommitAsync` is called

### Requirement: Infrastructure Tests Persist Through IUnitOfWork
The test suite SHALL exercise write-repository behavior the way production code is intended to use it: acquiring `BookWriteRepository`/`LoanWriteRepository` from an `IUnitOfWork` instance and persisting changes via `IUnitOfWork.CommitAsync()`, not by calling `DbContext.SaveChangesAsync()` directly in the "act" step of a test.

#### Scenario: Test commits via UnitOfWork
- **WHEN** a repository test exercises `AddBookAsync`, `RequestLoanAsync`, or `ReturnLoanAsync`
- **THEN** the test commits the resulting change via `unitOfWork.CommitAsync()` rather than `dbContext.SaveChangesAsync()`

### Requirement: BookReadRepository Coverage
The test suite SHALL exercise every code path of `BookReadRepository.GetBookByIdAsync` and `GetAllBooksAsync` against an ephemeral, in-process MongoDB instance, seeding documents directly into the `books` collection.

#### Scenario: Getting a book by id that exists
- **WHEN** `GetBookByIdAsync` is called with the `Id` of a `Book` document already seeded in the `books` collection
- **THEN** the matching `Book` is returned

#### Scenario: Getting a book by id that does not exist
- **WHEN** `GetBookByIdAsync` is called with a `Guid` that does not match any seeded `Book` document
- **THEN** `null` is returned

#### Scenario: Getting all books
- **WHEN** `GetAllBooksAsync` is called against a `books` collection with zero, one, or multiple seeded documents
- **THEN** it returns an empty collection, a single-item collection, or a multi-item collection matching what was seeded

### Requirement: LoanReadRepository Coverage
The test suite SHALL exercise every code path of `LoanReadRepository.GetAllLoansAsync` against an ephemeral, in-process MongoDB instance, seeding documents directly into the `loans` collection.

#### Scenario: Getting all loans
- **WHEN** `GetAllLoansAsync` is called against a `loans` collection with zero, one, or multiple seeded documents
- **THEN** it returns an empty collection, a single-item collection, or a multi-item collection matching what was seeded

### Requirement: Ephemeral MongoDB Test Infrastructure
The test suite SHALL provide a shared test fixture that starts an ephemeral, in-process MongoDB instance (via an embedded-Mongo test package) once per test class, exposes a real `IMongoDatabase` scoped to that instance for seeding and for constructing `BookReadRepository`/`LoanReadRepository`, and tears the instance down when the test class finishes, without requiring a live MongoDB server.

#### Scenario: Test class shares one ephemeral instance
- **WHEN** multiple `[Fact]` tests within `BookReadRepositoryTests` or `LoanReadRepositoryTests` run
- **THEN** they share a single ephemeral MongoDB instance for the test class rather than starting a new one per test method, and each test uses an isolated database/collection state so tests do not interfere with each other
