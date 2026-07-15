## Why

The `OpeaLibrary.Infrastructure` project (`src/Infrastruture/`) — the Postgres persistence layer (`OpeaLibraryDbContext`, EF Core entity configurations, `BookRepository`, `LoanRepository`) — has zero automated test coverage, and its source folder is misspelled (`Infrastruture` instead of `Infrastructure`, inconsistent with the project file and namespace, which are already spelled correctly). Reviewing the repositories against the Domain layer's business rules also surfaces two real defects in `LoanRepository`: (1) `RequestLoanAsync`/`ReturnLoanAsync` never call `Book.DecreaseQuantity()` / `Book.IncreaseQuantity()`, so `QuantityAvailable` is not adjusted when a loan is requested or returned; and (2) `ReturnLoanAsync`'s guard condition is inverted (`loan.ReturnDate.HasValue == false` instead of `loan.ReturnDate.HasValue`), which means the method can never actually mark an active loan as returned — it always returns `false` for an active loan and always throws for an already-returned one. Neither defect is caught by anything today, since the layer has no tests. Both need to be fixed and locked in with tests before this reaches production.

## What Changes

- Rename `src/Infrastruture/` to `src/Infrastructure/` and update `src/OpeaLibrary.slnx` (solution folder label and project path) to match. No namespace/csproj changes needed since those are already spelled correctly.
- Fix `LoanRepository` to enforce the Domain availability invariant, and to actually work at all:
  - `RequestLoanAsync`: call `book.DecreaseQuantity()` after successfully creating a `Loan`, instead of only reading `QuantityAvailable`.
  - `ReturnLoanAsync`: correct the inverted guard condition (`if (loan == null || loan.ReturnDate.HasValue) return false;`) so an active loan can actually be returned, then look up the associated `Book` via `loan.BookId` and call `book.IncreaseQuantity()` when the loan is marked as returned.
- Add a new xUnit test project (`OpeaLibrary.Infrastructure.Tests`) under `tests/`, referencing `src/Infrastructure/OpeaLibrary.Infrastructure.csproj`, using the EF Core InMemory provider to exercise `OpeaLibraryDbContext`-backed behavior without a real Postgres instance.
- Add unit tests covering 100% of the testable code in `src/Infrastructure/`:
  - `BookRepository`: `AddBookAsync`, `GetBookByIdAsync` (found + not-found), `GetAllBooksAsync` (empty + populated).
  - `LoanRepository`: `RequestLoanAsync` (success incl. quantity decrement, book not found, book with zero quantity), `ReturnLoanAsync` (success incl. quantity increment, loan not found, already-returned loan), `GetAllLoansAsync` (empty + populated).
  - `BookConfig` / `LoanConfig`: verify EF model metadata (table names, keys, required/max-length constraints) via the built `OpeaLibraryDbContext.Model`.
  - `OpeaLibraryDbContext`: verify `Books`/`Loans` `DbSet`s are wired and configurations from the assembly are applied.
- Configure code coverage collection (`coverlet.collector`, matching the Domain test project) so `dotnet test --collect:"XPlat Code Coverage"` can verify 100% line/branch coverage for `src/Infrastructure/`.
- Add the new test project to `src/OpeaLibrary.slnx` (or confirm `dotnet test` discovers it from `tests/` without a solution entry, consistent with the Domain test project).

## Capabilities

### New Capabilities
- `infrastructure-unit-tests`: Automated xUnit test suite validating the Postgres persistence layer (`BookRepository`, `LoanRepository`, EF Core configurations, `OpeaLibraryDbContext`) against the Domain layer's business rules, using an in-memory EF Core provider, with 100% code coverage as an enforced quality bar.

### Modified Capabilities
- None. `domain-unit-tests` is unaffected — no Domain test changes are needed for this change.

## Impact

- **Affected code**: `src/Infrastruture/` → renamed to `src/Infrastructure/`; `src/OpeaLibrary.slnx` updated; `LoanRepository.cs` behavior fixed to (1) call `DecreaseQuantity`/`IncreaseQuantity` and (2) correct the inverted `ReturnLoanAsync` guard condition that made the method non-functional for active loans.
- **New code**: `tests/OpeaLibrary.Infrastructure.Tests/` test project.
- **Dependencies**: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector` (mirroring the Domain test project), plus `Microsoft.EntityFrameworkCore.InMemory` (new, test-only dependency) for exercising `DbContext`-backed repositories without a real Postgres database.
- **Risk**: The `LoanRepository` behavior fixes are production code changes (bug fixes), not purely additive test scaffolding — flagged explicitly since they change runtime behavior (book quantities will now actually change on loan/return, and returning a loan will now actually succeed instead of always failing or throwing).
