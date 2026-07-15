## 1. Rename Infrastructure Folder

- [x] 1.1 `git mv src/Infrastruture src/Infrastructure`
- [x] 1.2 Update `src/OpeaLibrary.slnx`: folder label `/Infrastruture/` → `/Infrastructure/` and project path `Infrastruture/OpeaLibrary.Infrastructure.csproj` → `Infrastructure/OpeaLibrary.Infrastructure.csproj`
- [x] 1.3 Confirm `dotnet build` (or `dotnet build src/OpeaLibrary.slnx`) succeeds after the rename

## 2. Fix LoanRepository Business Rule Adherence

- [x] 2.1 In `RequestLoanAsync`, after confirming the book is available, call `book.DecreaseQuantity()` before returning `true` (no explicit `Update()` needed — the entity from `FindAsync` is already change-tracked)
- [x] 2.2 In `ReturnLoanAsync`, after `loan.MarkAsReturned()`, look up the book via `_dbContext.Books.FindAsync(loan.BookId)`; if found, call `book.IncreaseQuantity()` (no explicit `Update()` needed, same reason). **Additional fix found during implementation**: the guard condition was inverted (`loan.ReturnDate.HasValue == false` instead of `loan.ReturnDate.HasValue`), which made the method always return `false` for an active loan and always throw for an already-returned one — corrected to `if (loan == null || loan.ReturnDate.HasValue) return false;`
- [x] 2.3 Confirm the fix compiles and existing method signatures (`Task<bool>`) are unchanged

## 3. Infrastructure Test Project Setup

- [x] 3.1 Create `tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj` targeting `net10.0`
- [x] 3.2 Add NuGet packages: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`, `Microsoft.EntityFrameworkCore.InMemory`
- [x] 3.3 Add a project reference from `OpeaLibrary.Infrastructure.Tests` to `src/Infrastructure/OpeaLibrary.Infrastructure.csproj`
- [x] 3.4 Add `GlobalUsings.cs` (EF Core, Domain entities/interfaces, Infrastructure context/config/repository namespaces)
- [x] 3.5 Add a shared test helper to build a fresh `OpeaLibraryDbContext` backed by `UseInMemoryDatabase(Guid.NewGuid().ToString())` per test
- [x] 3.6 Verify `dotnet test tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj` runs successfully against the new (empty) project (there is no root-level `.sln`/`.slnx`, so `dotnet test` must target this `.csproj` explicitly — a bare `dotnet test` from the repo root fails with `MSB1003`). Note: build emits a harmless `MSB3277` version-unification warning (Npgsql's transitive `EntityFrameworkCore.Relational` 10.0.4 vs. the InMemory package's 10.0.10) — no functional impact since InMemory doesn't use the Relational package.

## 4. BookRepository Tests

- [x] 4.1 Create `tests/OpeaLibrary.Infrastructure.Tests/Repositories/BookRepositoryTests.cs`
- [x] 4.2 Test `AddBookAsync` persists a book retrievable after `SaveChangesAsync`
- [x] 4.3 Test `GetBookByIdAsync` returns the matching book when it exists
- [x] 4.4 Test `GetBookByIdAsync` returns `null` when no book matches
- [x] 4.5 Test `GetAllBooksAsync` returns an empty collection when no books are persisted
- [x] 4.6 Test `GetAllBooksAsync` returns all persisted books (single and multiple)

## 5. LoanRepository Tests

- [x] 5.1 Create `tests/OpeaLibrary.Infrastructure.Tests/Repositories/LoanRepositoryTests.cs`
- [x] 5.2 Test `RequestLoanAsync` returns `true`, persists an active `Loan`, and decrements the book's `QuantityAvailable` by 1
- [x] 5.3 Test `RequestLoanAsync` returns `false` and persists no loan when the book has `QuantityAvailable == 0`
- [x] 5.4 Test `RequestLoanAsync` returns `false` when the book id does not exist
- [x] 5.5 Test `ReturnLoanAsync` returns `true`, sets `ReturnDate`/`Status == Returned`, and increments the associated book's `QuantityAvailable` by 1
- [x] 5.6 Test `ReturnLoanAsync` returns `false` when the loan id does not exist
- [x] 5.7 Test `ReturnLoanAsync` returns `false` and does not change quantity again when the loan is already returned
- [x] 5.8 Test `GetAllLoansAsync` returns an empty collection when no loans are persisted
- [x] 5.9 Test `GetAllLoansAsync` returns all persisted loans (single and multiple)

## 6. EF Core Configuration and DbContext Tests

- [x] 6.1 Create `tests/OpeaLibrary.Infrastructure.Tests/Config/BookConfigTests.cs` asserting `OpeaLibraryDbContext.Model`'s `Book` entity type: table `tb_books`, key `Id`, `Title` required/max length 200, `Author` required/max length 100, `PublishedYear` and `QuantityAvailable` required
- [x] 6.2 Create `tests/OpeaLibrary.Infrastructure.Tests/Config/LoanConfigTests.cs` asserting `OpeaLibraryDbContext.Model`'s `Loan` entity type: table `tb_loans`, key `Id`, `BookId`/`LoanDate` required, `ReturnDate` optional
- [x] 6.3 Create `tests/OpeaLibrary.Infrastructure.Tests/Context/OpeaLibraryDbContextTests.cs` asserting `Books`/`Loans` `DbSet` properties are non-null and the model contains both entity types after `OnModelCreating`

## 7. Coverage Verification

- [x] 7.1 Run `dotnet test tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj --collect:"XPlat Code Coverage"` (there is no root-level `.sln`/`.slnx`; running bare `dotnet test` from the repo root fails with `MSB1003: Especifique um arquivo de solução ou de projeto`)
- [x] 7.2 Inspect the generated Cobertura report and confirm 100% line and branch coverage for `src/Infrastructure/Persistence/Postgres/**/*.cs`
- [x] 7.3 Add any missing test cases needed to close coverage gaps, then re-run until 100% is reached (added `ReturnLoanAsync_WhenAssociatedBookNoLongerExists_StillMarksReturned` to cover the `book?.IncreaseQuantity()` null-conditional branch)
- [x] 7.4 Confirm the full suite passes with no failing or skipped tests: run `dotnet test tests/OpeaLibrary.Domain.Tests/OpeaLibrary.Domain.Tests.csproj` and `dotnet test tests/OpeaLibrary.Infrastructure.Tests/OpeaLibrary.Infrastructure.Tests.csproj` individually (each targeted explicitly, since no aggregating solution file exists at the repo root) — 30/30 and 23/23 passing respectively
