## ADDED Requirements

### Requirement: xUnit Test Project for Domain Layer
The system SHALL provide an xUnit test project at `tests/OpeaLibrary.Domain.Tests/` that references `OpeaLibrary.Domain` and is runnable via `dotnet test`.

#### Scenario: Running the test suite
- **WHEN** a developer runs `dotnet test` against `tests/OpeaLibrary.Domain.Tests/OpeaLibrary.Domain.Tests.csproj`
- **THEN** all Domain unit tests execute and report pass/fail results using xUnit

### Requirement: Book Entity Coverage
The test suite SHALL exercise every code path of `Book.Create`, `Book.DecreaseQuantity`, and `Book.IncreaseQuantity`.

#### Scenario: Creating a book with valid data
- **WHEN** `Book.Create` is called with a non-empty title, non-empty author, and a positive published year
- **THEN** a `Book` is returned with a non-empty `Id`, the given `Title`, `Author`, `PublishedYear`, and `QuantityAvailable` equal to 0

#### Scenario: Creating a book with an empty or whitespace title
- **WHEN** `Book.Create` is called with a `title` that is null, empty, or whitespace
- **THEN** an `ArgumentException` is thrown referencing the `title` parameter

#### Scenario: Creating a book with an empty or whitespace author
- **WHEN** `Book.Create` is called with an `author` that is null, empty, or whitespace
- **THEN** an `ArgumentException` is thrown referencing the `author` parameter

#### Scenario: Creating a book with a non-positive published year
- **WHEN** `Book.Create` is called with `publishedYear` less than or equal to 0
- **THEN** an `ArgumentException` is thrown referencing the `publishedYear` parameter

#### Scenario: Decreasing quantity when copies are available
- **WHEN** `DecreaseQuantity` is called on a `Book` whose `QuantityAvailable` is greater than 0
- **THEN** `QuantityAvailable` is decremented by 1

#### Scenario: Decreasing quantity when no copies are available
- **WHEN** `DecreaseQuantity` is called on a `Book` whose `QuantityAvailable` is 0
- **THEN** an `InvalidOperationException` is thrown and `QuantityAvailable` remains unchanged

#### Scenario: Increasing quantity
- **WHEN** `IncreaseQuantity` is called on a `Book`
- **THEN** `QuantityAvailable` is incremented by 1

### Requirement: Loan Entity Coverage
The test suite SHALL exercise every code path of `Loan.Create` and `Loan.MarkAsReturned`, including the `StatusLoan` state transition.

#### Scenario: Creating a loan with valid data
- **WHEN** `Loan.Create` is called with a non-empty `bookId` and a non-empty `userId`
- **THEN** a `Loan` is returned with a non-empty `Id`, the given `BookId` and `UserId`, `LoanDate` set to approximately the current UTC time, `ReturnDate` equal to null, and `Status` equal to `StatusLoan.Active`

#### Scenario: Creating a loan with an empty book id
- **WHEN** `Loan.Create` is called with `bookId` equal to `Guid.Empty`
- **THEN** an `ArgumentException` is thrown referencing the `bookId` parameter

#### Scenario: Creating a loan with an empty user id
- **WHEN** `Loan.Create` is called with `userId` equal to `Guid.Empty`
- **THEN** an `ArgumentException` is thrown referencing the `userId` parameter

#### Scenario: Marking an active loan as returned
- **WHEN** `MarkAsReturned` is called on a `Loan` whose `ReturnDate` is null
- **THEN** `ReturnDate` is set to approximately the current UTC time and `Status` is set to `StatusLoan.Returned`

#### Scenario: Marking an already-returned loan as returned again
- **WHEN** `MarkAsReturned` is called on a `Loan` whose `ReturnDate` is already set
- **THEN** an `InvalidOperationException` is thrown and the loan's `ReturnDate`/`Status` remain unchanged

### Requirement: 100% Code Coverage of Domain Layer
The test suite SHALL achieve 100% line and branch coverage of all types under `src/Domain/Entities/` and `src/Domain/Enums/`, verifiable via `dotnet test --collect:"XPlat Code Coverage"`.

#### Scenario: Verifying coverage after running tests
- **WHEN** `dotnet test --collect:"XPlat Code Coverage"` is run against the Domain test project
- **THEN** the generated Cobertura coverage report shows 100% line coverage and 100% branch coverage for `src/Domain/Entities/*.cs`
