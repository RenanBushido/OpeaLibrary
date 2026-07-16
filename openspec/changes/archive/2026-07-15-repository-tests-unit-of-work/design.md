## Context

`src/Domain/Interfaces/IUnitOfWork.cs` defines:
```csharp
public interface IUnitOfWork
{
    IBookRepository BookRepository { get; }
    ILoanRepository LoanRepository { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
}
```
No class in `src/Infrastructure/` implements it. `BookRepository`/`LoanRepository` stage changes on `OpeaLibraryDbContext` but never call `SaveChangesAsync` themselves (this is documented in the archived `infrastructure-unit-tests` change's `design.md` as an intentional gap, left for a future `IUnitOfWork` implementation). `tests/OpeaLibrary.Infrastructure.Tests` currently:
- Instantiates repositories directly: `new BookRepository(dbContext)`, `new LoanRepository(dbContext)`.
- Persists by calling `dbContext.SaveChangesAsync()` directly, both for the "act" step (after calling a repository method) and for "arrange" steps (seeding preconditions via `dbContext.Books.AddAsync(...)` / `dbContext.Loans.AddAsync(...)`).

This means the tests validate repository logic against a raw `DbContext`, not against the abstraction (`IUnitOfWork`) that production code is expected to use — the interface exists but nothing exercises it.

## Goals / Non-Goals

**Goals:**
- Provide a concrete `UnitOfWork : IUnitOfWork` in `src/Infrastructure/Persistence/Postgres/`, the first implementation of this interface in the codebase.
- Rewrite every persistence call in `BookRepositoryTests`/`LoanRepositoryTests` so the "act" step goes through `IUnitOfWork.BookRepository`/`LoanRepository` and `IUnitOfWork.CommitAsync()`, not `new BookRepository(...)`/`new LoanRepository(...)` + `dbContext.SaveChangesAsync()`.
- Add direct tests for `UnitOfWork` itself.
- Preserve 100% line/branch coverage of `src/Infrastructure/Persistence/Postgres/**` (now including `UnitOfWork.cs`).

**Non-Goals:**
- No composition root / DI wiring is added (there still isn't an API/Presentation project) — `UnitOfWork` is only consumed from tests in this change, same as `BookRepository`/`LoanRepository` today.
- No change to `IBookRepository`, `ILoanRepository`, or `IUnitOfWork` contracts — this change only adds the missing implementation.
- No new repository methods (e.g. no generic "insert arbitrary `Loan`" method added to `ILoanRepository`) — where a test needs to seed a precondition the repository API can't express, direct `DbSet` access is kept for that specific seed (see Decision 3), just not for the commit itself.
- No transaction/isolation-level behavior beyond what `SaveChangesAsync` already provides — `CommitAsync` is a thin pass-through, not a new transactional boundary.

## Decisions

1. **Location: `src/Infrastructure/Persistence/Postgres/UnitOfWork.cs`, sibling to `Repositories/`, `Config/`, `Context/`.**
   `UnitOfWork` is a Postgres-specific implementation of a Domain contract, exactly like `BookRepository`/`LoanRepository` are Postgres-specific implementations of `IBookRepository`/`ILoanRepository`. It sits one level up from `Repositories/` (not inside it) because it composes both repositories rather than being one itself — mirroring how `Context/OpeaLibraryDbContext.cs` also sits at that level as shared infrastructure.
   - Alternative considered: put it inside `Repositories/` alongside `BookRepository`/`LoanRepository` — rejected since it isn't itself a repository implementing an `I*Repository` contract; grouping it there would blur that distinction.

2. **Eager construction of `BookRepository`/`LoanRepository` in `UnitOfWork`'s primary constructor, not lazy (`??=`) properties.**
   ```csharp
   public sealed class UnitOfWork(OpeaLibraryDbContext dbContext) : IUnitOfWork
   {
       public IBookRepository BookRepository { get; } = new BookRepository(dbContext);
       public ILoanRepository LoanRepository { get; } = new LoanRepository(dbContext);

       public Task CommitAsync(CancellationToken cancellationToken = default) =>
           dbContext.SaveChangesAsync(cancellationToken);
   }
   ```
   Constructing `BookRepository`/`LoanRepository` is cheap (they only wrap a `DbContext` reference), so lazy backing fields would add boilerplate without a real benefit. This also matches the primary-constructor style already used by `BookRepository`/`LoanRepository`.
   - Alternative considered: lazy `??=` properties — rejected as unnecessary complexity for objects this cheap to construct.

3. **Test seeding: route through `IUnitOfWork` wherever the repository API supports it; keep direct `DbSet` access only where it doesn't, but always commit via `CommitAsync`.**
   - `IBookRepository.AddBookAsync(Book)` can express every `Book` seed needed today, so all `Book` seeding in tests moves to `unitOfWork.BookRepository.AddBookAsync(book)`.
   - `ILoanRepository` has no method to insert an arbitrary, already-constructed `Loan` — only `RequestLoanAsync(bookId)`, which always creates a *new* `Loan` via `Loan.Create` and requires an existing book with available quantity. Two existing tests need a `Loan` the repository API cannot produce this way: `ReturnLoanAsync_WhenAssociatedBookNoLongerExists_StillMarksReturned` (an orphaned loan, no book at all) and `GetAllLoansAsync_WhenLoansPersisted_ReturnsAllLoans` (two freestanding loans, no book needed). For these, keep `dbContext.Loans.AddAsync(loan)` for the seed itself, but replace the following `dbContext.SaveChangesAsync()` with `unitOfWork.CommitAsync()`.
   - Alternative considered: add an `AddLoanAsync` to `ILoanRepository` so every seed could go through a repository method — rejected as scope creep; the user's ask was to fix how saves are performed, not to expand the repository contract. Flagged as a Non-Goal.

4. **`UnitOfWorkTests.cs` placed at the test project root (`tests/OpeaLibrary.Infrastructure.Tests/UnitOfWorkTests.cs`), not under `Repositories/`.**
   Mirrors the production file's location one level up from `Repositories/`.
   - Covers: `BookRepository`/`LoanRepository` properties are non-null and operate against the same `DbContext` (an entity added through `unitOfWork.BookRepository` is visible via `dbContext.Books` before `CommitAsync`, proving they share state); `CommitAsync` persists staged changes (a `Book` added via `AddBookAsync` without committing is not yet in a fresh read from the context — not applicable for `UseInMemoryDatabase` since it's the same context instance, so instead assert that calling `CommitAsync` does not throw and that data set up via `unitOfWork.BookRepository.AddBookAsync` remains queryable through `unitOfWork.BookRepository.GetBookByIdAsync` after `CommitAsync`).

## Risks / Trade-offs

- [Risk] `UnitOfWork` is new production code with no consumer other than tests (still no composition root). → Mitigation: it is a thin, low-risk wrapper (two property getters + one pass-through method); behavior is fully covered by `UnitOfWorkTests.cs`, and the existing `LoanRepository`/`BookRepository` behavior is untouched.
- [Risk] Two seed scenarios in `LoanRepositoryTests` still bypass the repository API (direct `dbContext.Loans.AddAsync`), which could look inconsistent with the "always use `IUnitOfWork`" intent. → Mitigation: documented explicitly in Decision 3 and called out in code comments at each occurrence, so it reads as a deliberate, scoped exception rather than an oversight.
- [Risk] 100% coverage could regress if `UnitOfWork.CommitAsync` or a property getter goes unexercised. → Mitigation: `UnitOfWorkTests.cs` exercises both properties and `CommitAsync` directly; re-run `dotnet test --collect:"XPlat Code Coverage"` after the change and inspect the Cobertura report before considering the change done (same verification pattern used in `infrastructure-unit-tests`).
