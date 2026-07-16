## Context

`src/Application/` (project `OpeaLibrary.Application`, targeting `net10.0`, referencing `AutoMapper` 16.2.0, `FluentValidation` 12.1.1, `MediatR` 14.2.0, and `OpeaLibrary.Domain`) implements CQRS for two aggregates:
- **Books**: `AddBookCommand`/`Handler`/`Validator`, `GetAllBookRequest`/`Handler`, `GetBookByIdRequest`/`Handler`/`Validator`.
- **Loans**: `RequestLoanCommand`/`Handler`/`Validator`, `ReturnLoanCommand` (file misnamed `ReturnLoanCommnad.cs`)/`Handler`/`Validator`, `GetAllLoansRequest`/`Handler`.
- `Mappings/MappingProfile.cs`: AutoMapper profile mapping `Book`→`GetBookByIdResponse`/`GetAllBookResponse` and `Loan`→`GetAllLoansResponse`, each with `.ReverseMap()`.
- `Common/Queries/`, `Common/Commands/`: empty directories, no shared abstractions defined yet.

Reviewing the handlers against `IUnitOfWork`/`IBookRepository`/`ILoanRepository` (Domain) and their Postgres implementations (Infrastructure, from the archived `repository-tests-unit-of-work` change) surfaced:
- `RequestLoanCommandHandler.Handle` calls `await _unitOfWork.LoanRepository.RequestLoanAsync(request.BookId);` and discards the `bool`, then `return true;` unconditionally.
- `ReturnLoanCommandHandler.Handle` has the identical shape: discards `ReturnLoanAsync`'s `bool`, `return true;` unconditionally.
- Both still call `CommitAsync` even when the repository call reported failure (nothing was staged, so this is wasteful but not incorrect on its own — the real bug is the swallowed `bool`).
- `GetBookByIdValidator` exists (validates `Id != Guid.Empty`) but `GetBookByIdQueryHandler.Handle` never constructs or calls it — every other handler with a validator (`AddBookCommandHandler`, `RequestLoanCommandHandler`, `ReturnLoanCommandHandler`) manually does `new XxxValidator().Validate(request)` then throws `ValidationException` on failure; `GetBookByIdQueryHandler` simply omits this step.
- `IBookRepository.GetBookByIdAsync`/`GetAllBooksAsync` and `ILoanRepository.GetAllLoansAsync` now require a `CancellationToken` parameter (added outside any tracked OpenSpec change, alongside the Application layer's own development). `tests/OpeaLibrary.Infrastructure.Tests` was not updated for this and currently fails to build with 12 `CS7036` errors (missing required argument) across `BookRepositoryTests.cs`, `LoanRepositoryTests.cs`, and `UnitOfWorkTests.cs`.

There is still no API/Presentation project or composition root anywhere in the repository — `OpeaLibrary.Application` has no consumer yet, same situation as `IUnitOfWork` before the `repository-tests-unit-of-work` change gave it an implementation.

## Goals / Non-Goals

**Goals:**
- Fix the two loan-handler bugs so the CQRS layer's contract (`IRequestHandler<RequestLoanCommand, bool>` / `IRequestHandler<ReturnLoanCommand, bool>`) actually reflects repository outcome.
- Replace duplicated manual per-handler validation with a single `ValidationBehavior<TRequest, TResponse>` MediatR pipeline behavior, applied uniformly to every request (fixing the `GetBookByIdValidator` gap as a side effect of uniformity, not a special case).
- Add `src/Application/DependencyInjection.cs` so a future composition root has one call (`services.AddApplication()`) to wire MediatR, AutoMapper, the 4 validators, and the pipeline behavior.
- Restore `tests/OpeaLibrary.Infrastructure.Tests` to a compiling, passing state (mechanical fix only, matching the already-specified `infrastructure-unit-tests` behavior).
- Stand up `tests/OpeaLibrary.Application.Tests` (xUnit + Moq) covering every handler, validator, the pipeline behavior, and the mapping profile, with 100% line/branch coverage of `src/Application/`.

**Non-Goals:**
- No API/Presentation project or actual DI container wiring beyond the extension method itself — there's still nothing to call `AddApplication()` from.
- No changes to `Common/Queries/`/`Common/Commands/` (empty, unused) — left as-is.
- No changes to `IBookRepository`/`ILoanRepository`/`IUnitOfWork` contracts or their Postgres implementations — the Infrastructure fix only touches test call sites, not production Infrastructure code.
- No pagination, sorting, or filtering added to `GetAllBookRequest`/`GetAllLoansRequest` — out of scope, they remain parameterless "get everything" queries.
- No integration tests spanning Application→Infrastructure→a real/InMemory database — Application tests fake the repository/unit-of-work abstractions; that end-to-end path is already covered at the Infrastructure layer by `repository-tests-unit-of-work`.

## Decisions

1. **Fix loan handlers to propagate the repository's `bool` result, and only commit when it succeeded.**
   ```csharp
   public async Task<bool> Handle(RequestLoanCommand request, CancellationToken cancellationToken)
   {
       var succeeded = await _unitOfWork.LoanRepository.RequestLoanAsync(request.BookId);
       if (!succeeded) return false;
       await _unitOfWork.CommitAsync(cancellationToken);
       return true;
   }
   ```
   Same shape for `ReturnLoanCommandHandler`. Manual validation is removed from both (see Decision 2), so the method body becomes: validate is no longer inline, call repository, branch on result, commit only on success.
   - Alternative considered: keep `CommitAsync` unconditional (harmless since nothing is staged on failure) — rejected in favor of the branch anyway, since it makes the success/failure semantics of the handler explicit to a reader instead of relying on "well, nothing was staged so it's a no-op".

2. **`ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>`, registered as an open generic behavior, replacing all manual per-handler validation.**
   ```csharp
   public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
       : IPipelineBehavior<TRequest, TResponse>
       where TRequest : notnull
   {
       public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
       {
           if (validators.Any())
           {
               var context = new ValidationContext<TRequest>(request);
               var failures = (await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                   .SelectMany(result => result.Errors)
                   .Where(failure => failure is not null)
                   .ToList();

               if (failures.Count != 0)
                   throw new ValidationException(failures);
           }

           return await next(cancellationToken);
       }
   }
   ```
   (Exact `RequestHandlerDelegate<TResponse>` invocation syntax and generic constraint to be confirmed against the installed MediatR 14.2.0 API at implementation time — MediatR's pipeline behavior shape has changed across major versions; the constraint may need to be `where TRequest : IRequest<TResponse>` instead of `notnull` depending on version, per MediatR's own migration notes.) Registered via `cfg.AddOpenBehavior(typeof(ValidationBehavior<,>))` inside `AddApplication()`, so it runs for every request uniformly — `AddBookCommand`, `RequestLoanCommand`, `ReturnLoanCommand`, and `GetBookByIdRequest` all get validated the same way, with zero special-casing. `GetAllBookRequest`/`GetAllLoansRequest` have no registered validator, so `validators.Any()` is `false` and they pass straight through.
   - Alternative considered: keep manual per-handler validation but just add the missing call to `GetBookByIdQueryHandler` — rejected; it would leave the duplicated boilerplate in place and reintroduce the same "someone forgot to call the validator" risk for the next handler added.
   - Alternative considered: `IRequestPreProcessor<TRequest>` instead of `IPipelineBehavior` — rejected; pipeline behavior is the standard place to short-circuit the pipeline by throwing, and is what the ecosystem (and this design's Context7-verified MediatR docs) demonstrates for validation specifically.

3. **`DependencyInjection.cs` registers validators explicitly (`services.AddScoped<IValidator<AddBookCommand>, AddBookValidator>()`, one line per validator), not via `FluentValidation.DependencyInjectionExtensions`' assembly-scanning `AddValidatorsFromAssembly`.**
   Only 4 validators exist; explicit registration avoids adding a new NuGet package for what a few lines of manual registration already cover, and keeps `AddApplication()` legible as documentation of exactly what's registered.
   - Alternative considered: add `FluentValidation.DependencyInjectionExtensions` and scan the assembly — rejected as an unnecessary dependency for 4 known validators; revisit if the validator count grows significantly.
   - MediatR registration: `services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly); cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); });` — MediatR's own `AddMediatR` extension is already available from the referenced `MediatR` package (no separate DI package needed, confirmed via MediatR's migration docs — this was folded into the core package since MediatR 12).
   - AutoMapper registration: `services.AddAutoMapper(typeof(MappingProfile).Assembly);` — assumed available directly from the referenced `AutoMapper` 16.2.0 package; verify at implementation time and add `AutoMapper.Extensions.Microsoft.DependencyInjection` only if the main package doesn't expose it at this version.

4. **Rename `ReturnLoanCommnad.cs` → `ReturnLoanCommand.cs`** (content/class name already correct — `ReturnLoanCommand` — only the filename has the typo). Matches the `Infrastruture`→`Infrastructure` folder-rename precedent from an earlier change: fix the cosmetic naming defect alongside substantive work in the same area, don't leave it for a separate change.

5. **Infrastructure test compile fix: pass `cancellationToken`/`CancellationToken.None` (or a `default` where no token is naturally in scope) at each of the 12 call sites**, no other changes to those test files. This restores the already-passing, already-specified `infrastructure-unit-tests` capability to a compiling state; it is not a behavior or requirements change (the `infrastructure-unit-tests` spec already describes `GetBookByIdAsync`/`GetAllBooksAsync`/`GetAllLoansAsync` behavior — nothing about their signature — so no delta spec is needed for this capability).

6. **`OpeaLibrary.Application.Tests` uses Moq to fake `IUnitOfWork`, `IBookRepository`, `ILoanRepository`, and (where a handler needs it only for pass-through, not real mapping behavior) `IMapper`; query handler tests that assert actual mapped output build a real `IMapper` from `new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())` instead of mocking the mapping itself.**
   Handlers depend entirely on interfaces (`IUnitOfWork`, `IBookRepository`, `ILoanRepository`, `IMapper`) rather than a concrete `DbContext`, so — unlike the Infrastructure layer, where a real (InMemory) `DbContext` was preferred over mocking EF Core's APIs — mocking these narrow, already-abstract interfaces is the standard, low-ceremony approach for testing orchestration/validation logic in a CQRS layer. Using a *real* `IMapper` (not a mock) for query handlers ensures the `MappingProfile` itself is exercised and its actual output shape is asserted, rather than a mocked `IMapper.Map<T>()` call trivially returning whatever the test tells it to.
   - Alternative considered: NSubstitute — equally valid, but Moq is the more common default for xUnit projects and no mocking library exists yet in this repo, so either is a green-field choice; Moq is picked per the user's preference.
   - Alternative considered: hand-written fakes (no new dependency) — rejected; with 3 repository-shaped interfaces plus `IMapper`, Moq's `Mock<T>().Setup(...)` is materially less boilerplate than maintaining bespoke fake classes for each interface, and this is exactly the scenario mocking libraries are designed for (unlike the EF Core `DbContext` case, which Microsoft's own docs discourage mocking).

## Risks / Trade-offs

- [Risk] `RequestLoanCommandHandler`/`ReturnLoanCommandHandler` fixes change observable behavior for any future caller: they will now correctly receive `false` for failed loan operations instead of always `true`. → Mitigation: there is no current caller (no API project), so this has zero blast radius today; called out explicitly in the proposal's Impact section for when a caller is eventually added.
- [Risk] Introducing `ValidationBehavior` changes when validation errors surface (now before the handler body runs, via the MediatR pipeline, rather than as the first lines inside the handler) — any code that currently catches exceptions from calling a handler directly (bypassing MediatR's `Send`) would stop seeing validation errors. → Mitigation: there is no such caller today (everything goes through `IMediator.Send`, in tests via a manually constructed pipeline or MediatR's DI-resolved sender); tests exercise the behavior explicitly rather than relying on incidental handler-level validation.
- [Risk] Exact `MediatR` 14.2.0 `IPipelineBehavior`/`RequestHandlerDelegate` signature might differ slightly from the pseudocode in Decision 2 (MediatR has changed this shape across major versions per its own migration docs). → Mitigation: verify against the installed package (IntelliSense/compiler errors) during implementation; the design's intent (validate-then-either-throw-or-call-next) is what must be preserved, not the exact syntax shown here.
- [Risk] `services.AddAutoMapper(...)` extension availability assumed for AutoMapper 16.2.0 without having confirmed it against that exact version's package contents. → Mitigation: if compilation fails, add `AutoMapper.Extensions.Microsoft.DependencyInjection` (or construct/register `IMapper` via `MapperConfiguration` manually) as a fallback — noted here so it isn't a surprise mid-implementation.
- [Risk] The Infrastructure test compile fix and the Application layer work are bundled in one change, which is somewhat unrelated concerns bundled together. → Mitigation: user explicitly opted to bundle them (asked during proposal) since the broken Infrastructure tests block having a fully green `tests/` folder alongside the new Application tests; kept as a small, mechanical, clearly-delineated task group rather than mixed into the Application-layer tasks.
