## 1. Fix Loan Handler Bugs

- [x] 1.1 In `src/Application/Loans/Commands/RequestLoan/RequestLoanCommandHandler.cs`, capture the `bool` result of `_unitOfWork.LoanRepository.RequestLoanAsync(request.BookId)`, return `false` immediately if it's `false`, and only call `CommitAsync` + return `true` on success
- [x] 1.2 In `src/Application/Loans/Commands/ReturnLoan/ReturnLoanCommandHandler.cs`, apply the same fix for `_unitOfWork.LoanRepository.ReturnLoanAsync(request.LoanId)`
- [x] 1.3 Confirm `dotnet build src/Application/OpeaLibrary.Application.csproj` succeeds

## 2. Introduce ValidationBehavior Pipeline

- [x] 2.1 Create `src/Application/Behaviors/ValidationBehavior.cs` implementing `IPipelineBehavior<TRequest, TResponse>`, resolving `IEnumerable<IValidator<TRequest>>`, validating, and throwing `FluentValidation.ValidationException` on failure before calling `next` (signature matched the installed MediatR 14.2.0 API on first try, no adjustment needed)
- [x] 2.2 Remove the manual `new XxxValidator().Validate(request)` + throw block from `AddBookCommandHandler`, `RequestLoanCommandHandler`, and `ReturnLoanCommandHandler` (validation now happens in the pipeline, before `Handle` runs)
- [x] 2.3 Confirm `GetBookByIdValidator` is not called directly anywhere in `GetBookByIdQueryHandler` (it will be invoked automatically by the pipeline once registered in task 3)
- [x] 2.4 Confirm `dotnet build src/Application/OpeaLibrary.Application.csproj` succeeds

## 3. Add DependencyInjection Extension

- [x] 3.1 Create `src/Application/DependencyInjection.cs` with a `public static IServiceCollection AddApplication(this IServiceCollection services)` extension method
- [x] 3.2 Register MediatR handlers from the Application assembly and register `ValidationBehavior<,>` as an open generic pipeline behavior
- [x] 3.3 Register AutoMapper with `MappingProfile`
- [x] 3.4 Register each of the 4 validators explicitly (`IValidator<AddBookCommand>` → `AddBookValidator`, `IValidator<GetBookByIdRequest>` → `GetBookByIdValidator`, `IValidator<RequestLoanCommand>` → `RequestLoanValidator`, `IValidator<ReturnLoanCommand>` → `ReturnLoanValidator`)
- [x] 3.5 Add `Microsoft.Extensions.DependencyInjection.Abstractions` package reference to `OpeaLibrary.Application.csproj` if not already available transitively (confirmed already available transitively via MediatR/AutoMapper 10.0.0 — no explicit reference needed)
- [x] 3.6 Confirm `dotnet build src/Application/OpeaLibrary.Application.csproj` succeeds

## 4. Fix Naming and Restore Infrastructure Test Compilation

- [x] 4.1 Rename `src/Application/Loans/Commands/ReturnLoan/ReturnLoanCommnad.cs` to `ReturnLoanCommand.cs` (no content change beyond the filename)
- [x] 4.2 In `tests/OpeaLibrary.Infrastructure.Tests/Repositories/BookRepositoryTests.cs`, pass a `CancellationToken` argument (e.g. `CancellationToken.None` or `default`) at every `GetBookByIdAsync`/`GetAllBooksAsync` call site
- [x] 4.3 In `tests/OpeaLibrary.Infrastructure.Tests/Repositories/LoanRepositoryTests.cs`, pass a `CancellationToken` argument at every `GetBookByIdAsync`/`GetAllLoansAsync` call site
- [x] 4.4 In `tests/OpeaLibrary.Infrastructure.Tests/UnitOfWorkTests.cs`, pass a `CancellationToken` argument at the `GetBookByIdAsync` call site
- [x] 4.5 Run `dotnet test tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj` and confirm it builds and all tests pass (25/25 expected, no behavior change)

## 5. Application Test Project Setup

- [x] 5.1 Create `tests/OpeaLibrary.Application.Tests/OpeaLibrary.Application.Tests.csproj` targeting `net10.0`
- [x] 5.2 Add NuGet packages: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`, `Moq`
- [x] 5.3 Add a project reference from `OpeaLibrary.Application.Tests` to `src/Application/OpeaLibrary.Application.csproj`
- [x] 5.4 Add `GlobalUsings.cs` (MediatR, FluentValidation, AutoMapper, Moq, Domain entities/interfaces, Application namespaces)
- [x] 5.5 Verify `dotnet test` runs successfully against the new (empty) project

## 6. Command Handler Tests

- [x] 6.1 Create `tests/OpeaLibrary.Application.Tests/Books/Commands/AddBookCommandHandlerTests.cs`: valid command creates a `Book`, calls `AddBookAsync` + `CommitAsync`, returns the new `Id`
- [x] 6.2 Create `tests/OpeaLibrary.Application.Tests/Loans/Commands/RequestLoanCommandHandlerTests.cs`: success case (returns `true`, commits) and failure case (returns `false`, never commits)
- [x] 6.3 Create `tests/OpeaLibrary.Application.Tests/Loans/Commands/ReturnLoanCommandHandlerTests.cs`: success case (returns `true`, commits) and failure case (returns `false`, never commits)

## 7. Query Handler Tests

- [x] 7.1 Create `tests/OpeaLibrary.Application.Tests/Books/Queries/GetAllBookQueryHandlerTests.cs` using a real `IMapper` built from `MappingProfile`
- [x] 7.2 Create `tests/OpeaLibrary.Application.Tests/Books/Queries/GetBookByIdQueryHandlerTests.cs`
- [x] 7.3 Create `tests/OpeaLibrary.Application.Tests/Loans/Queries/GetAllLoansQueryHandlerTests.cs`

## 8. Validator Tests

- [x] 8.1 Create `tests/OpeaLibrary.Application.Tests/Books/Commands/AddBookValidatorTests.cs` (all `[Theory]` cases for invalid Title/Author/PublishedYear, plus a valid case)
- [x] 8.2 Create `tests/OpeaLibrary.Application.Tests/Books/Queries/GetBookByIdValidatorTests.cs`
- [x] 8.3 Create `tests/OpeaLibrary.Application.Tests/Loans/Commands/RequestLoanValidatorTests.cs`
- [x] 8.4 Create `tests/OpeaLibrary.Application.Tests/Loans/Commands/ReturnLoanValidatorTests.cs`

## 9. ValidationBehavior and MappingProfile Tests

- [x] 9.1 Create `tests/OpeaLibrary.Application.Tests/Behaviors/ValidationBehaviorTests.cs`: passing validation calls `next` once; failing validation throws `ValidationException` and never calls `next`; no registered validator calls `next` once
- [x] 9.2 Create `tests/OpeaLibrary.Application.Tests/Mappings/MappingProfileTests.cs`: `AssertConfigurationIsValid()`, plus one concrete mapping assertion per declared map (`Book`→`GetBookByIdResponse`, `Book`→`GetAllBookResponse`, `Loan`→`GetAllLoansResponse`)

## 10. Coverage and Full Suite Verification

- [x] 10.1 Run `dotnet test tests/OpeaLibrary.Application.Tests/OpeaLibrary.Application.Tests.csproj --collect:"XPlat Code Coverage"`
- [x] 10.2 Inspect the generated Cobertura report and confirm 100% line and branch coverage for `src/Application/**/*.cs`
- [x] 10.3 Add any missing test cases needed to close coverage gaps, then re-run until 100% is reached (added `DependencyInjectionTests` to cover `DependencyInjection.cs`, which was at 0% line coverage; everything else was already 100%)
- [x] 10.4 Run all three test projects individually (`tests/OpeaLibrary.Domain.Tests`, `tests/OpeaLibrary.Infrastructure.Tests`, `tests/OpeaLibrary.Application.Tests`) and confirm no failing tests (28/28, 25/25, 32/32 — all green, 0 skipped)

## 11. Post-Review Fixes (sdd-review findings, resolved before archive)

- [x] 11.1 `Book.Create` had no way to set an initial `QuantityAvailable`, so a newly added book could never be loaned (only `ReturnLoanAsync` calls `IncreaseQuantity`, which requires an active loan to exist first — a bootstrap deadlock). Added an optional `quantityAvailable = 0` parameter to `Book.Create` with a `DomainException` guard for negative values; added `QuantityAvailable` to `AddBookCommand`, wired it through `AddBookCommandHandler`, and added an `AddBookValidator` rule (`GreaterThanOrEqualTo(0)`). Added Domain.Tests (`Create_WithNegativeQuantityAvailable_ThrowsDomainException`, `Create_WithPositiveQuantityAvailable_SetsQuantityAvailable`) and updated Application.Tests (`AddBookCommandHandlerTests`, `AddBookValidatorTests`) accordingly. Domain.Tests 30/30, Application.Tests 33/33, both 100% coverage maintained.
- [x] 11.2 `GetBookByIdQueryHandler` silently returned AutoMapper's `null` for a non-existent book despite its non-nullable `IRequestHandler<GetBookByIdRequest, GetBookByIdResponse>` contract. Changed the handler to throw `KeyNotFoundException` when `IBookRepository.GetBookByIdAsync` returns `null`. Added the "Getting a book by id that does not exist" scenario to `specs/application-unit-tests/spec.md` and a corresponding test in `GetBookByIdQueryHandlerTests.cs`. Application.Tests 34/34, 100% coverage maintained.
- [x] 11.3 `LoanConfig` had no foreign key relationship from `Loan.BookId` to `Book`, so nothing enforced referential integrity in the real Postgres schema. Added `builder.HasOne<Book>().WithMany().HasForeignKey(l => l.BookId).OnDelete(DeleteBehavior.Restrict)` to `LoanConfig.cs`. Confirmed via `LoanRepositoryTests.ReturnLoanAsync_WhenAssociatedBookNoLongerExists_StillMarksReturned` (an intentional, spec'd orphaned-loan scenario) that this doesn't break existing tests — the EF Core InMemory provider doesn't enforce FK constraints, only the real Postgres provider will. Added `LoanEntity_HasForeignKeyRelationshipToBook` to `LoanConfigTests.cs` and a `MODIFIED Requirements` delta to `specs/infrastructure-unit-tests/spec.md` (this change now modifies the `infrastructure-unit-tests` capability, not just `application-unit-tests`). Infrastructure.Tests 26/26, 100% coverage maintained.
- [x] 11.4 Removed the empty, unused `src/Application/Common/Queries/` and `src/Application/Common/Commands/` directories flagged as structural noise during review.
