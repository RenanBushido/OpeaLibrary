## Why

The `OpeaLibrary.Domain` project (`src/Domain/`) contains the core business rules for the library system — entity creation, validation, and state transitions for `Book` and `Loan` — but has no automated test coverage. Without tests, regressions in domain rules (e.g., quantity going negative, double-returning a loan) can slip through unnoticed as the codebase grows.

## What Changes

- Add a new xUnit test project (`OpeaLibrary.Domain.Tests`) under `tests/`, referencing `src/Domain/OpeaLibrary.Domain.csproj`.
- Add unit tests covering 100% of the testable code in `src/Domain/`:
  - `Book`: `Create` (valid + all validation failure paths), `DecreaseQuantity` (success + failure when zero), `IncreaseQuantity`.
  - `Loan`: `Create` (valid + all validation failure paths), `MarkAsReturned` (success + failure when already returned).
  - `StatusLoan` enum: value coverage exercised indirectly through `Loan` behavior tests.
- Configure code coverage collection (via `coverlet.collector`) so coverage can be measured and verified to reach 100% line/branch coverage for `src/Domain/Entities` and `src/Domain/Enums`.
- Wire the new test project into the solution (create/update a `.sln` if one does not already exist, or otherwise ensure `dotnet test` picks it up from `tests/`).

## Capabilities

### New Capabilities
- `domain-unit-tests`: Automated xUnit test suite validating the behavior and invariants of the Domain layer (`Book`, `Loan`, `Entity`, `StatusLoan`), with 100% code coverage as an enforced quality bar.

### Modified Capabilities
- None. This change adds tests only; no production `src/Domain` behavior changes.

## Impact

- **Affected code**: `src/Domain/` (read-only, no changes expected unless a testability gap is found, e.g. sealing/visibility).
- **New code**: `tests/OpeaLibrary.Domain.Tests/` test project.
- **Dependencies**: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector` (NuGet packages added to the new test project).
- **Tooling**: `dotnet test` (and optionally a coverage report step) becomes the way to validate the Domain layer going forward.
