# application-unit-tests Specification

## Purpose
Automated xUnit test suite validating the Application layer's CQRS handlers, validators, the `ValidationBehavior` pipeline, and `MappingProfile`, using Moq to fake `IUnitOfWork`, `IBookReadRepository`, `IBookWriteRepository`, `ILoanReadRepository`, and `ILoanWriteRepository`, and a real AutoMapper `IMapper` built from `MappingProfile`. 100% line and branch coverage of `src/Application/` is the quality bar for this suite.

## Requirements

### Requirement: xUnit Test Project for Application Layer
The system SHALL provide an xUnit test project at `tests/OpeaLibrary.Application.Tests/` that references `OpeaLibrary.Application` and is runnable via `dotnet test`, using Moq to fake `IUnitOfWork`, `IBookReadRepository`, `IBookWriteRepository`, `ILoanReadRepository`, and `ILoanWriteRepository`, and a real AutoMapper `IMapper` built from `MappingProfile`.

#### Scenario: Running the test suite
- **WHEN** a developer runs `dotnet test` against `tests/OpeaLibrary.Application.Tests/OpeaLibrary.Application.Tests.csproj`
- **THEN** all Application unit tests execute and report pass/fail results using xUnit, with no external database or API host required

### Requirement: RequestLoanCommandHandler Propagates Repository Result
`RequestLoanCommandHandler` SHALL return the actual result of `ILoanWriteRepository.RequestLoanAsync` (accessed via `IUnitOfWork.LoanWriteRepository`), and SHALL only call `IUnitOfWork.CommitAsync` when that result is `true`.

#### Scenario: Requesting a loan succeeds
- **WHEN** `RequestLoanCommandHandler.Handle` is called and the faked `ILoanWriteRepository.RequestLoanAsync` returns `true`
- **THEN** the handler returns `true` and `IUnitOfWork.CommitAsync` is called exactly once

#### Scenario: Requesting a loan fails
- **WHEN** `RequestLoanCommandHandler.Handle` is called and the faked `ILoanWriteRepository.RequestLoanAsync` returns `false`
- **THEN** the handler returns `false` and `IUnitOfWork.CommitAsync` is never called

### Requirement: ReturnLoanCommandHandler Propagates Repository Result
`ReturnLoanCommandHandler` SHALL return the actual result of `ILoanWriteRepository.ReturnLoanAsync` (accessed via `IUnitOfWork.LoanWriteRepository`), and SHALL only call `IUnitOfWork.CommitAsync` when that result is `true`.

#### Scenario: Returning a loan succeeds
- **WHEN** `ReturnLoanCommandHandler.Handle` is called and the faked `ILoanWriteRepository.ReturnLoanAsync` returns `true`
- **THEN** the handler returns `true` and `IUnitOfWork.CommitAsync` is called exactly once

#### Scenario: Returning a loan fails
- **WHEN** `ReturnLoanCommandHandler.Handle` is called and the faked `ILoanWriteRepository.ReturnLoanAsync` returns `false`
- **THEN** the handler returns `false` and `IUnitOfWork.CommitAsync` is never called

### Requirement: AddBookCommandHandler Coverage
The test suite SHALL exercise `AddBookCommandHandler`, asserting it creates a `Book` via the Domain factory, adds it through `IUnitOfWork.BookWriteRepository.AddBookAsync`, commits via `IUnitOfWork.CommitAsync`, and returns the new book's `Id`.

#### Scenario: Adding a valid book
- **WHEN** `AddBookCommandHandler.Handle` is called with a valid `AddBookCommand`
- **THEN** `IUnitOfWork.BookWriteRepository.AddBookAsync` and `IUnitOfWork.CommitAsync` are each called exactly once, and the returned `Guid` matches the created `Book`'s `Id`

### Requirement: Query Handler Coverage
The test suite SHALL exercise `GetAllBooksQueryHandler`, `GetBookByIdQueryHandler`, and `GetAllLoansQueryHandler`, asserting each maps read-repository results to its response type via the real `MappingProfile`.

#### Scenario: Getting all books
- **WHEN** `GetAllBooksQueryHandler.Handle` is called and the faked `IBookReadRepository.GetAllBooksAsync` returns a collection of `Book`
- **THEN** the handler returns a `GetAllBooksResponse` for each `Book`, with fields mapped correctly

#### Scenario: Getting a book by id
- **WHEN** `GetBookByIdQueryHandler.Handle` is called and the faked `IBookReadRepository.GetBookByIdAsync` returns a `Book`
- **THEN** the handler returns a `GetBookByIdResponse` with fields mapped correctly

#### Scenario: Getting a book by id that does not exist
- **WHEN** `GetBookByIdQueryHandler.Handle` is called and the faked `IBookReadRepository.GetBookByIdAsync` returns `null`
- **THEN** the handler throws `KeyNotFoundException` instead of returning a null response

#### Scenario: Getting all loans
- **WHEN** `GetAllLoansQueryHandler.Handle` is called and the faked `ILoanReadRepository.GetAllLoansAsync` returns a collection of `Loan`
- **THEN** the handler returns a `GetAllLoansResponse` for each `Loan`, with fields mapped correctly

### Requirement: Validator Coverage
The test suite SHALL exercise every validation rule in `AddBookValidator`, `GetBookByIdValidator`, `RequestLoanValidator`, and `ReturnLoanValidator`, for both valid and invalid inputs.

#### Scenario: AddBookValidator rejects invalid input
- **WHEN** `AddBookValidator` validates an `AddBookCommand` with an empty/too-long `Title`, empty/too-long `Author`, or a `PublishedYear` outside `1000..DateTime.Now.Year`
- **THEN** validation fails with an error for the corresponding field

#### Scenario: AddBookValidator accepts valid input
- **WHEN** `AddBookValidator` validates an `AddBookCommand` with a non-empty `Title` (≤200 chars), non-empty `Author` (≤100 chars), and `PublishedYear` within range
- **THEN** validation succeeds with no errors

#### Scenario: GetBookByIdValidator, RequestLoanValidator, ReturnLoanValidator reject an empty Guid
- **WHEN** any of these validators validates a request whose `Id`/`BookId`/`LoanId` is `Guid.Empty`
- **THEN** validation fails with an error for that field

#### Scenario: GetBookByIdValidator, RequestLoanValidator, ReturnLoanValidator accept a non-empty Guid
- **WHEN** any of these validators validates a request whose `Id`/`BookId`/`LoanId` is a non-empty `Guid`
- **THEN** validation succeeds with no errors

### Requirement: ValidationBehavior Pipeline Coverage
The test suite SHALL verify that `ValidationBehavior<TRequest, TResponse>` invokes the next delegate when validation succeeds or when no validator is registered for `TRequest`, and throws `FluentValidation.ValidationException` without invoking the next delegate when validation fails.

#### Scenario: Valid request passes through
- **WHEN** `ValidationBehavior.Handle` is called for a request with a registered validator that passes
- **THEN** the next delegate is invoked exactly once and its result is returned

#### Scenario: Invalid request short-circuits the pipeline
- **WHEN** `ValidationBehavior.Handle` is called for a request with a registered validator that fails
- **THEN** a `ValidationException` is thrown and the next delegate is never invoked

#### Scenario: Request with no registered validator passes through
- **WHEN** `ValidationBehavior.Handle` is called for a request type with no registered `IValidator<TRequest>`
- **THEN** the next delegate is invoked exactly once without attempting validation

### Requirement: MappingProfile Coverage
The test suite SHALL verify that `MappingProfile`'s AutoMapper configuration is valid and that each declared map produces the expected field values.

#### Scenario: Mapping configuration is valid
- **WHEN** a `MapperConfiguration` is built from `MappingProfile` and `AssertConfigurationIsValid()` is called
- **THEN** no exception is thrown

#### Scenario: Book maps to GetBookByIdResponse and GetAllBooksResponse
- **WHEN** a `Book` is mapped to `GetBookByIdResponse` and to `GetAllBooksResponse`
- **THEN** `Id`, `Title`, `Author`, `PublishedYear`, and `QuantityAvailable` match the source `Book`'s values in both cases

#### Scenario: Loan maps to GetAllLoansResponse
- **WHEN** a `Loan` is mapped to `GetAllLoansResponse`
- **THEN** `Id`, `BookId`, `LoanDate`, `ReturnDate`, and `Status` match the source `Loan`'s values

### Requirement: 100% Code Coverage of Application Layer
The test suite SHALL achieve 100% line and branch coverage of all types under `src/Application/`, verifiable via `dotnet test --collect:"XPlat Code Coverage"`.

#### Scenario: Verifying coverage after running tests
- **WHEN** `dotnet test --collect:"XPlat Code Coverage"` is run against the Application test project
- **THEN** the generated Cobertura coverage report shows 100% line coverage and 100% branch coverage for `src/Application/**/*.cs`
