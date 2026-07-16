## Why

The CQRS split (`BookWriteRepository`/`LoanWriteRepository` on Postgres, `BookReadRepository`/`LoanReadRepository` on MongoDB) has no mechanism connecting the two sides. Verified directly while fixing database connectivity in a prior change: `POST /api/books` persists a row to `tb_books` successfully, but `GET /api/books`/`GET /api/books/{id}` never sees it, because the Mongo `books`/`loans` collections are never written to by anything — `AddBookCommandHandler`, `RequestLoanCommandHandler`, and `ReturnLoanCommandHandler` all commit to Postgres and stop there. The read side is permanently empty regardless of how much data is written. This makes the application's core read paths non-functional.

## What Changes

- Add a domain events mechanism to `src/Domain`: `Entity` gains a `DomainEvents` collection (`AddDomainEvent`/`ClearDomainEvents`), and a new `IDomainEvent` marker interface. `Book.Create`/`DecreaseQuantity`/`IncreaseQuantity` and `Loan.Create`/`MarkAsReturned` each raise the corresponding event (`BookCreatedEvent`, `BookQuantityChangedEvent`, `LoanCreatedEvent`, `LoanReturnedEvent`) — pure domain-layer additions, no new dependencies.
- Add a `DomainEventsInterceptor` (EF Core `SaveChangesInterceptor`) in `src/Infrastructure`, registered on `OpeaLibraryDbContext` via `AddInfraPostgres`, that — after a **successful** `SaveChangesAsync` — walks the `ChangeTracker`'s tracked entities, collects and clears their `DomainEvents`, and publishes each one through `IMediator` (MediatR's existing notification pipeline; no new message broker or background worker).
- Add MongoDB projection notification handlers in `src/Infrastructure` (`IBookReadRepository`/`ILoanReadRepository` stay read-only per CQRS convention — these handlers write directly via `IMongoCollection<Book>`/`IMongoCollection<Loan>`, kept internal to Infrastructure) that react to each domain event and upsert/update the corresponding `books`/`loans` Mongo documents.
- Extend MediatR's assembly scanning (`AddApiMediatR`/`AddInfraMongo` in `src/CrossCutting`) to also register handlers from the `OpeaLibrary.Infrastructure` assembly, since the new projection handlers live there, not in `OpeaLibrary.Application` (the only assembly currently scanned).
- **No changes to `AddBookCommandHandler`, `RequestLoanCommandHandler`, `ReturnLoanCommandHandler`, or any Application-layer contract** — the sync is transparent: entities raise events as a side effect of their existing factory/mutation methods, and dispatch happens automatically inside `UnitOfWork.CommitAsync` via the interceptor. Command handlers, their tests, and their specs are unaffected.

## Capabilities

### New Capabilities
- `cqrs-read-model-sync`: The mechanism that keeps MongoDB's read-side collections (`books`, `loans`) consistent with Postgres's write-side tables after every successful command, via domain events raised by `Book`/`Loan` and dispatched through MediatR after each successful `SaveChangesAsync`.

### Modified Capabilities
- `domain-unit-tests`: `Book`/`Loan`/`Entity` gain new, testable behavior (raising and clearing domain events) that the existing domain test suite's 100% coverage bar now needs to cover.
- `infrastructure-unit-tests`: New requirements for `DomainEventsInterceptor` (dispatches events only after a successful save, clears them off entities, does not dispatch on a failed save) and the new Mongo projection notification handlers (each event type produces the expected `books`/`loans` document state).
- `crosscutting-unit-tests`: `ApplicationExtensions Registration Coverage` now also requires verifying that `AddApiMediatR`'s assembly scan discovers the new Infrastructure-layer notification handlers, not just Application-layer request handlers — the single most important gotcha in this design (MediatR silently no-ops on zero registered handlers rather than erroring).

## Impact

- **Affected code**: `src/Domain/Entities/Entity.cs`, `Book.cs`, `Loan.cs` (raise/clear domain events); `src/CrossCutting/Extensions/ApplicationExtensions.cs` (`AddApiMediatR`'s assembly scan now also covers `OpeaLibrary.Infrastructure`); `src/Infrastructure/Persistence/Postgres/...` (new `DomainEventsInterceptor`, wired into `AddInfraPostgres`).
- **New code**: `src/Domain/Events/` (`IDomainEvent`, `BookCreatedEvent`, `BookQuantityChangedEvent`, `LoanCreatedEvent`, `LoanReturnedEvent`); `src/Infrastructure/Persistence/Postgres/Interceptors/DomainEventsInterceptor.cs`; `src/Infrastructure/Persistence/Mongodb/EventHandlers/` (one notification handler per event, projecting into `books`/`loans`).
- **Risk**: The interceptor's `SavedChangesAsync` hook runs *after* Postgres has already committed — if a Mongo projection write inside it fails, the exception propagates back through `SaveChangesAsync` to the calling command handler, which would report the whole operation as failed even though the Postgres write already succeeded (a real inconsistency-vs-error-reporting trade-off of the in-process/synchronous approach chosen here over an outbox+background-worker alternative). `design.md` covers the accepted mitigation. No data migration risk — this only affects newly-written data going forward; it does not backfill any data written to Postgres before this change (there is none yet, per the prior connectivity-fix change's verification).
