## 1. UnitOfWork Implementation

- [x] 1.1 Create `src/Infrastructure/Persistence/Postgres/UnitOfWork.cs` implementing `IUnitOfWork`: `BookRepository`/`LoanRepository` properties constructed eagerly from the injected `OpeaLibraryDbContext`, `CommitAsync` wrapping `dbContext.SaveChangesAsync(cancellationToken)`
- [x] 1.2 Confirm `dotnet build src/Infrastructure/OpeaLibrary.Infrastructure.csproj` succeeds

## 2. UnitOfWork Tests

- [x] 2.1 Create `tests/OpeaLibrary.Infrastructure.Tests/UnitOfWorkTests.cs`
- [x] 2.2 Test `BookRepository` and `LoanRepository` properties are non-null and backed by the same `DbContext` (add a book via `unitOfWork.BookRepository.AddBookAsync`, then verify it is queryable via `dbContext.Books` before `CommitAsync`)
- [x] 2.3 Test `CommitAsync` persists staged changes (add a book via `unitOfWork.BookRepository.AddBookAsync`, call `CommitAsync`, then verify it is retrievable via `unitOfWork.BookRepository.GetBookByIdAsync`)

## 3. Rewrite BookRepositoryTests to Use IUnitOfWork

- [x] 3.1 In `tests/OpeaLibrary.Infrastructure.Tests/Repositories/BookRepositoryTests.cs`, replace `new BookRepository(dbContext)` with `new UnitOfWork(dbContext).BookRepository` (or a local `unitOfWork` variable exposing `.BookRepository`) in every test
- [x] 3.2 Replace every `await dbContext.SaveChangesAsync();` in this file with `await unitOfWork.CommitAsync();`
- [x] 3.3 Replace direct `dbContext.Books.AddAsync(...)` / `AddRangeAsync(...)` seed calls with `unitOfWork.BookRepository.AddBookAsync(...)` calls followed by `unitOfWork.CommitAsync()` (the repository API fully supports these seeds)
- [x] 3.4 Run `dotnet test tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj` and confirm all `BookRepositoryTests` still pass

## 4. Rewrite LoanRepositoryTests to Use IUnitOfWork

- [x] 4.1 In `tests/OpeaLibrary.Infrastructure.Tests/Repositories/LoanRepositoryTests.cs`, replace `new LoanRepository(dbContext)` with a `unitOfWork.LoanRepository` reference in every test; use `unitOfWork.BookRepository` wherever a book is also involved
- [x] 4.2 Replace every `await dbContext.SaveChangesAsync();` in this file with `await unitOfWork.CommitAsync();`
- [x] 4.3 Replace direct `dbContext.Books.AddAsync(...)` seed calls with `unitOfWork.BookRepository.AddBookAsync(...)` + `unitOfWork.CommitAsync()`
- [x] 4.4 For the two seeds `ILoanRepository` cannot express (`ReturnLoanAsync_WhenAssociatedBookNoLongerExists_StillMarksReturned`'s orphaned loan, and `GetAllLoansAsync_WhenLoansPersisted_ReturnsAllLoans`'s freestanding loans), keep the direct `dbContext.Loans.AddAsync(...)` call but commit it via `unitOfWork.CommitAsync()`, with a short comment noting why direct `DbSet` access remains here
- [x] 4.5 Run `dotnet test tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj` and confirm all `LoanRepositoryTests` still pass

## 5. Coverage and Full Suite Verification

- [x] 5.1 Run `dotnet test tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj --collect:"XPlat Code Coverage"`
- [x] 5.2 Inspect the generated Cobertura report and confirm 100% line and branch coverage for `src/Infrastructure/Persistence/Postgres/**/*.cs`, including the new `UnitOfWork.cs`
- [x] 5.3 Add any missing test cases needed to close coverage gaps, then re-run until 100% is reached (none needed — 100% reached on first pass)
- [x] 5.4 Confirm no remaining `dbContext.SaveChangesAsync()` calls exist anywhere in `tests/OpeaLibrary.Infrastructure.Tests/Repositories/` (grep for it)
- [x] 5.5 Run the full suite (`dotnet test tests/OpeaLibrary.Domain.Tests/OpeaLibrary.Domain.Tests.csproj` and `dotnet test tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj` individually) and confirm no failing or skipped tests — 28/28 and 25/25 passing respectively
