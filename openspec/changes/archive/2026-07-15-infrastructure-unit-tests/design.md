## Context

`src/Infrastruture/` (project name `OpeaLibrary.Infrastructure`, targeting `net10.0`) implements the Postgres persistence layer:
- `Persistence/Postgres/Context/OpeaLibraryDbContext.cs` — `DbContext` with `Books`/`Loans` `DbSet`s, applies `IEntityTypeConfiguration<T>` classes from its own assembly.
- `Persistence/Postgres/Config/BookConfig.cs`, `LoanConfig.cs` — EF Core fluent configuration (table names, keys, required/max-length constraints).
- `Persistence/Postgres/Repositories/BookRepository.cs`, `LoanRepository.cs` — implement `IBookRepository`/`ILoanRepository` from the Domain layer.
- `Persistence/Mongodb/` — empty directory, no code yet; out of scope.

There is no test project for this layer today, and no real Postgres instance is available in this environment. The folder is misspelled (`Infrastruture`) even though the `.csproj` file, namespace, and assembly name are already `OpeaLibrary.Infrastructure` — only `src/OpeaLibrary.slnx` references the wrong folder name.

Comparing `LoanRepository` against the Domain rules in `src/Domain/Entities/Book.cs` (`DecreaseQuantity`/`IncreaseQuantity`) and `Loan.cs` (`Create`/`MarkAsReturned`) shows the repository does not call these methods when it should:
- `RequestLoanAsync` reads `book.QuantityAvailable` directly to decide whether a loan is possible, but never decrements it — a book's available count never goes down after a loan.
- `ReturnLoanAsync` marks the loan returned but never looks up the book to increment its `QuantityAvailable` back up.

This is a genuine adherence gap between Infrastructure and Domain, not just a coverage gap, and needs a production fix alongside the tests.

A second, more severe defect was found while implementing that fix: `ReturnLoanAsync`'s guard condition is inverted —
`if (loan == null || loan.ReturnDate.HasValue == false) return false;` — which reads as "bail out unless the loan has already been returned." In practice this means:
- For an active loan (`ReturnDate == null`), the condition is `true`, so the method returns `false` immediately — an active loan can never be returned.
- For an already-returned loan (`ReturnDate` has a value), the condition is `false`, so execution falls through to `loan.MarkAsReturned()`, which throws `DomainException` because `ReturnDate != null`.

So, as written, `ReturnLoanAsync` could never succeed for any input — it always either returns `false` or throws. This was not caught by the original review because no tests existed to exercise it. It is fixed alongside the quantity-adjustment gap, since both are required for the method to match its intended contract (see Decision 3).

## Goals / Non-Goals

**Goals:**
- Rename `src/Infrastruture/` → `src/Infrastructure/` and repoint `src/OpeaLibrary.slnx` at the new path/label.
- Fix `LoanRepository` so `RequestLoanAsync`/`ReturnLoanAsync` actually invoke `Book.DecreaseQuantity()`/`IncreaseQuantity()` and persist the updated `Book`, matching the invariant the Domain layer defines.
- Stand up `tests/OpeaLibrary.Infrastructure.Tests/` (xUnit, mirroring the Domain test project's conventions) covering `BookRepository`, `LoanRepository`, `BookConfig`, `LoanConfig`, and `OpeaLibraryDbContext`.
- Achieve 100% line and branch coverage of `src/Infrastructure/Persistence/Postgres/**`, verifiable via `dotnet test --collect:"XPlat Code Coverage"`.
- Use an EF Core provider that requires no external database (no Postgres, no Docker/Testcontainers) so tests run anywhere `dotnet test` runs.

**Non-Goals:**
- No changes to `Persistence/Mongodb/` (empty, unimplemented) — out of scope.
- No integration tests against a real PostgreSQL instance (e.g., Testcontainers) — out of scope for this change; the in-memory provider is sufficient to validate repository logic and EF configuration metadata.
- No CI pipeline / coverage-gate enforcement — consistent with the Domain test project, only local `dotnet test` verification is in scope.
- No changes to `IBookRepository`/`ILoanRepository`/`IUnitOfWork` contracts in `src/Domain` — the fix stays inside `LoanRepository`'s implementation.
- No `IUnitOfWork` implementation is added. None exists in `src/Infrastructure/` today, and no code anywhere in the repo calls `SaveChangesAsync` (verified via a repo-wide search) — there is no composition root (no API/Presentation project, no DI wiring) yet. Repository methods (`AddBookAsync`, `RequestLoanAsync`, etc.) only stage changes on the `DbContext`; committing them is intentionally left to whatever future `IUnitOfWork` implementation and application layer calls `CommitAsync()`. Tests call `SaveChangesAsync()` directly on the test's `OpeaLibraryDbContext` to observe persisted state, bypassing the (currently nonexistent) `IUnitOfWork` — this is a test-only shortcut, not a statement that the production commit path works end-to-end.

## Decisions

1. **Test provider: EF Core InMemory (`Microsoft.EntityFrameworkCore.InMemory`), not Sqlite or a real Postgres.**
   Repository tests construct a real `OpeaLibraryDbContext` backed by `UseInMemoryDatabase(Guid.NewGuid().ToString())` (unique name per test to avoid cross-test state bleed). This exercises the actual repository code paths (`AddAsync`, `FindAsync`, `ToListAsync`, `Update`) without mocking EF Core's `DbSet`/`DbContext` APIs, which is brittle and doesn't verify real query/tracking behavior.
   - Alternative considered: Sqlite in-memory (`Filename=:memory:`) — closer to real relational behavior (enforces max length, etc.) but requires manually managing an open `DbConnection` for the DB's lifetime and adds a package/complexity not needed here, since these tests target repository logic and EF *configuration metadata*, not runtime constraint enforcement. Rejected.
   - Alternative considered: mocking `DbContext`/`DbSet` with a mocking library (e.g., Moq) — rejected as brittle and low-value; a real (in-memory) `DbContext` is simpler and exercises more real code.
   - Alternative considered: Testcontainers + real Postgres — most realistic, but adds a Docker dependency to the test run and is disproportionate for this change's scope. Rejected for now; can be revisited if Postgres-specific behavior (e.g., `uuid` column type enforcement) ever needs verification.

2. **`BookConfig`/`LoanConfig` are tested via the built model's metadata, not by asserting on the configuration classes in isolation.**
   A `OpeaLibraryDbContext` is instantiated (with the InMemory provider) and `context.Model.FindEntityType(typeof(Book))` / `FindEntityType(typeof(Loan))` is inspected for table name, key, and property facets (`IsRequired`, `GetMaxLength()`). This both executes `Configure(...)` (for coverage) and validates the actual applied configuration, which is more meaningful than invoking `Configure` against a bare `ModelBuilder` and asserting nothing ran.
   - Alternative considered: instantiate `EntityTypeBuilder<T>` directly and call `Configure` — rejected because it requires more setup ceremony to get a working `ModelBuilder`/`EntityTypeBuilder` than just building the real `DbContext`, for no extra fidelity.

3. **Production bug fixes included in this change: `LoanRepository.RequestLoanAsync`/`ReturnLoanAsync` now call `Book.DecreaseQuantity()`/`IncreaseQuantity()`, and `ReturnLoanAsync`'s inverted guard condition is corrected.**
   Since the whole point of this change is to write tests that verify adherence to Domain business rules, writing a test that documents the *current* (wrong) behavior — quantity never changes, and returns never succeed — would be actively misleading. The repository is fixed so behavior matches the Domain invariant, and tests assert the fixed behavior.
   - `RequestLoanAsync`: after confirming the book is available, call `book.DecreaseQuantity()`. No explicit `_dbContext.Books.Update(book)` call is needed: `book` was obtained via `FindAsync`, so it is already tracked by the `DbContext`'s change tracker, and `SaveChangesAsync()` (whenever it is eventually called) will pick up the mutated `QuantityAvailable` automatically. An explicit `Update()` here would be redundant ceremony, not a correctness requirement.
   - `ReturnLoanAsync`: correct the guard condition from `if (loan == null || loan.ReturnDate.HasValue == false) return false;` to `if (loan == null || loan.ReturnDate.HasValue) return false;` (see Context — the original always returned `false` for an active loan and always threw for an already-returned one). Then, after `loan.MarkAsReturned()`, look up the book via `_dbContext.Books.FindAsync(loan.BookId)` and call `book.IncreaseQuantity()` — no explicit `Update()` needed since the entity is already tracked. If the book is somehow missing (orphaned `BookId`), the loan is still marked returned — the domain rule only requires quantity to be restored when the book exists, and no repository interface currently allows signaling a partial failure here.
   - Alternative considered: leave the bugs as-is and only test current behavior — rejected; the user explicitly asked to check adherence to Domain business rules, and shipping tests that lock in known-wrong behavior would defeat that purpose. This would also have been impossible in practice for the inverted condition, since no test could assert a `true` return from `ReturnLoanAsync` without the fix.
   - Alternative considered: push the decrement/increment into a higher application/service layer instead of the repository — there is no application/service layer in this codebase yet (repositories are called directly per `ILoanRepository`), so the fix stays at the level the bug was introduced.

4. **Folder rename via `git mv`, not delete+recreate**, to preserve file history. Only `src/OpeaLibrary.slnx` needs a content update (folder label `/Infrastruture/` → `/Infrastructure/`, project path `Infrastruture/OpeaLibrary.Infrastructure.csproj` → `Infrastructure/OpeaLibrary.Infrastructure.csproj`). No `.cs`/`.csproj` files reference the old folder name (namespace and assembly name were already correct).

5. **Coverage tool and project conventions mirror `OpeaLibrary.Domain.Tests`**: `coverlet.collector`, `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, one test class per source type (`Repositories/BookRepositoryTests.cs`, `Repositories/LoanRepositoryTests.cs`, `Config/BookConfigTests.cs`, `Config/LoanConfigTests.cs`, `Context/OpeaLibraryDbContextTests.cs`), plus a `GlobalUsings.cs`.

## Risks / Trade-offs

- [Risk] The `LoanRepository` fix is a behavior change to production code, not purely additive test scaffolding. → Mitigation: called out explicitly in the proposal's Impact section; tests cover both the fixed decrement/increment behavior and the pre-existing not-found/zero-quantity/already-returned branches so no behavior is unintentionally altered.
- [Risk] EF Core InMemory provider doesn't enforce relational constraints (e.g., max length, required) at write time, so a test asserting "saving a too-long title throws" would not work with it. → Mitigation: constraint verification is done by asserting on `Model` metadata (facet values) rather than by attempting writes that should fail — sidesteps the limitation entirely.
- [Risk] `Persistence/Mongodb/` is an empty folder with no code — if it's a stray scaffold left from initial setup rather than intentional, 100% coverage is trivially satisfied (nothing to cover) but the empty folder itself may be worth flagging to the user separately. → Mitigation: left untouched; out of scope for this change, mentioned here for visibility only.
- [Risk] 100% coverage target could be missed if a branch (e.g., the "book not found on return" fallback) is hard to trigger without deleting a row mid-test. → Mitigation: seed the in-memory `DbContext` with a `Loan` whose `BookId` does not correspond to any seeded `Book` to exercise that path directly.
- [Risk] No `IUnitOfWork` implementation or composition root exists in this repo, so passing tests here do not prove the production commit path (`CommitAsync` → `SaveChangesAsync`) actually works — only that repository logic is correct once changes are saved. → Mitigation: out of scope for this change (see Non-Goals); flagged so it isn't mistaken for end-to-end coverage. A future change should add the `IUnitOfWork` implementation and an integration test exercising it.
- [Risk] There is no root-level `.sln`/`.slnx` aggregating the test projects, so `dotnet test` must target each `.csproj` explicitly (confirmed: a bare `dotnet test` from the repo root fails with `MSB1003`). → Mitigation: `tasks.md` specifies explicit `.csproj` paths for every `dotnet test` invocation.
