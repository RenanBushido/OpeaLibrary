## Why

`IUnitOfWork` (`src/Domain/Interfaces/IUnitOfWork.cs`) exists to be the single entry point for persisting changes across repositories, but it has zero concrete implementation anywhere in `src/Infrastructure/`. As a result, `OpeaLibrary.Infrastructure.Tests` bypasses it entirely: every test that needs to persist data calls `OpeaLibraryDbContext.SaveChangesAsync()` directly, and repositories are instantiated directly (`new BookRepository(dbContext)`) instead of being obtained through a unit of work. This was already flagged as a known risk when the Infrastructure test suite was first built (see the archived `infrastructure-unit-tests` change's `design.md`), and the interface remains dead code without it. The tests should exercise the persistence layer the way production code is meant to use it.

## What Changes

- Add a concrete `UnitOfWork` class in `src/Infrastructure/Persistence/Postgres/` implementing `IUnitOfWork`: exposes `BookRepository`/`LoanRepository` backed by the same `OpeaLibraryDbContext`, and `CommitAsync` wraps `_dbContext.SaveChangesAsync(cancellationToken)`.
- Update `BookRepositoryTests` and `LoanRepositoryTests` to obtain repositories via an `IUnitOfWork` instance (`unitOfWork.BookRepository` / `unitOfWork.LoanRepository`) and persist via `unitOfWork.CommitAsync()`, replacing every direct `dbContext.SaveChangesAsync()` call in these test classes.
- Where a test needs to seed a precondition that no repository method can construct (e.g. a specific pre-built `Loan`, since `ILoanRepository` has no generic "insert this loan" method, only `RequestLoanAsync(bookId)` which always creates its own new `Loan`), keep the direct `DbSet.AddAsync(...)` call for that seed, but still commit it via `unitOfWork.CommitAsync()` instead of `dbContext.SaveChangesAsync()`.
- Add tests for the new `UnitOfWork` class itself: `BookRepository`/`LoanRepository` are non-null and backed by the same context, and `CommitAsync` actually persists staged changes.
- Extend coverage verification to keep 100% line/branch coverage of `src/Infrastructure/Persistence/Postgres/**` including the new `UnitOfWork.cs`.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `infrastructure-unit-tests`: adds a requirement that a concrete `IUnitOfWork` implementation exists for the Postgres persistence layer, and that the Infrastructure test suite persists changes through it instead of calling `DbContext.SaveChangesAsync()` directly.

## Impact

- **Affected code**: `tests/OpeaLibrary.Infrastructure.Tests/Repositories/BookRepositoryTests.cs` and `LoanRepositoryTests.cs` (persistence calls rewritten to go through `IUnitOfWork`); `tests/OpeaLibrary.Infrastructure.Tests/GlobalUsings.cs` (new namespace).
- **New code**: `src/Infrastructure/Persistence/Postgres/UnitOfWork.cs` (production code — this is the first concrete implementation of `IUnitOfWork`); `tests/OpeaLibrary.Infrastructure.Tests/UnitOfWorkTests.cs`.
- **No changes** to `IBookRepository`, `ILoanRepository`, `IUnitOfWork`, `BookRepository`, or `LoanRepository` behavior — this change only adds the missing `IUnitOfWork` implementation and fixes how tests use it.
- **Risk**: `UnitOfWork` is new production code, not just test scaffolding — it will be the first thing outside this test suite that could wire it into a future composition root (still absent), so its `CommitAsync` behavior (a thin wrapper over `SaveChangesAsync`) needs to be correct now.
