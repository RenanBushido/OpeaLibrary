## Context

`src/Domain/` (`OpeaLibrary.Domain`, targeting `net10.0`) currently has no test project. It contains:
- `Entity` (abstract base with `Id`)
- `Book` (factory `Create` + `DecreaseQuantity`/`IncreaseQuantity`)
- `Loan` (factory `Create` + `MarkAsReturned`)
- `StatusLoan` (enum: `Active`, `Returned`)

There is a `tests/` directory at the repo root but it is currently empty. No solution (`.sln`) file exists yet at the repo root. The user wants xUnit tests with 100% coverage for everything under `src/Domain/`.

## Goals / Non-Goals

**Goals:**
- Stand up an xUnit test project under `tests/OpeaLibrary.Domain.Tests/` referencing `OpeaLibrary.Domain`.
- Achieve 100% line and branch coverage of `src/Domain/Entities/*.cs` (and exercise `StatusLoan` values through those tests).
- Make coverage verifiable locally via `dotnet test --collect:"XPlat Code Coverage"`.
- Keep the test project self-contained and runnable with a single `dotnet test` command.

**Non-Goals:**
- Do not modify production behavior in `src/Domain` unless a genuine testability blocker is found (e.g., a private setter that cannot be reached — none identified so far, since all mutation happens through public factory/behavior methods).
- Do not add integration, API, or infrastructure-layer tests — out of scope for this change.
- Do not enforce coverage thresholds in CI/build pipeline (no CI config exists yet in this repo) — only local verification is in scope.

## Decisions

1. **Test framework: xUnit** — explicitly requested by the user. Packages: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`.
   - Alternative considered: MSTest/NUnit — rejected, user explicitly asked for xUnit.

2. **Coverage tool: coverlet.collector** — integrates with `dotnet test --collect:"XPlat Code Coverage"` out of the box, no extra global tool install required. Produces Cobertura XML that can be inspected or fed into a report generator.
   - Alternative considered: `dotnet-coverage` global tool — rejected to keep the test project self-contained (NuGet package vs. machine-wide tool).

3. **Project layout**: `tests/OpeaLibrary.Domain.Tests/OpeaLibrary.Domain.Tests.csproj`, mirroring the `src/Domain` folder structure with `Entities/BookTests.cs`, `Entities/LoanTests.cs`. `Entity` is abstract with no independent behavior, so it is covered implicitly via `Book`/`Loan` tests rather than a dedicated test file. `StatusLoan` is a plain enum with no logic, so it is covered implicitly via `Loan` behavior assertions (`Status` transitions from `Active` to `Returned`).
   - Alternative considered: a single flat `DomainTests.cs` — rejected in favor of one test class per entity for clarity and maintainability.

4. **Test style**: one `[Fact]` per distinct behavior/branch, plus `[Theory]`/`[InlineData]` for validation-argument variants (e.g., empty/whitespace/null title, author) to minimize duplication while still hitting every branch.

5. **No solution file required for `dotnet test`**: `dotnet test` can run directly against the test project's `.csproj`. A `.sln` is not required for this change; if the repo later adds one, the test project should be included in it, but creating a `.sln` is out of scope unless the build breaks without it.

## Risks / Trade-offs

- [Risk] `DateTime.UtcNow` is used directly in `Loan.Create`/`MarkAsReturned`, making exact-time assertions flaky. → Mitigation: assert with a tolerance window (e.g., `Assert.True((DateTime.UtcNow - loan.LoanDate) < TimeSpan.FromSeconds(5))`) rather than exact equality.
- [Risk] 100% coverage target could be missed if a branch is unreachable through the public API (none currently identified). → Mitigation: run `dotnet test --collect:"XPlat Code Coverage"` and inspect the Cobertura report after writing tests; add missing cases before declaring the task complete.
- [Risk] No existing `.sln`/CI wiring means coverage could silently regress later since nothing enforces it automatically. → Mitigation: out of scope for this change per Non-Goals, but noted for a future change if desired.
