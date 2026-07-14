## 1. Test Project Setup

- [x] 1.1 Create `tests/OpeaLibrary.Domain.Tests/OpeaLibrary.Domain.Tests.csproj` targeting `net10.0`
- [x] 1.2 Add NuGet packages: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`
- [x] 1.3 Add a project reference from `OpeaLibrary.Domain.Tests` to `src/Domain/OpeaLibrary.Domain.csproj`
- [x] 1.4 Verify `dotnet test` runs successfully against the new (empty) project

## 2. Book Entity Tests

- [x] 2.1 Create `tests/OpeaLibrary.Domain.Tests/Entities/BookTests.cs`
- [x] 2.2 Test `Book.Create` with valid data returns a `Book` with expected `Id`, `Title`, `Author`, `PublishedYear`, and `QuantityAvailable == 0`
- [x] 2.3 Test `Book.Create` throws `ArgumentException` for null/empty/whitespace `title` (use `[Theory]`/`[InlineData]`)
- [x] 2.4 Test `Book.Create` throws `ArgumentException` for null/empty/whitespace `author` (use `[Theory]`/`[InlineData]`)
- [x] 2.5 Test `Book.Create` throws `ArgumentException` for `publishedYear <= 0` (use `[Theory]`/`[InlineData]` for 0 and negative values)
- [x] 2.6 Test `DecreaseQuantity` decrements `QuantityAvailable` when copies are available
- [x] 2.7 Test `DecreaseQuantity` throws `InvalidOperationException` when `QuantityAvailable == 0`
- [x] 2.8 Test `IncreaseQuantity` increments `QuantityAvailable`

## 3. Loan Entity Tests

- [x] 3.1 Create `tests/OpeaLibrary.Domain.Tests/Entities/LoanTests.cs`
- [x] 3.2 Test `Loan.Create` with valid data returns a `Loan` with expected `Id`, `BookId`, `UserId`, `LoanDate` (within tolerance of `DateTime.UtcNow`), `ReturnDate == null`, and `Status == StatusLoan.Active`
- [x] 3.3 Test `Loan.Create` throws `ArgumentException` when `bookId == Guid.Empty`
- [x] 3.4 Test `Loan.Create` throws `ArgumentException` when `userId == Guid.Empty`
- [x] 3.5 Test `MarkAsReturned` sets `ReturnDate` (within tolerance of `DateTime.UtcNow`) and `Status == StatusLoan.Returned` on an active loan
- [x] 3.6 Test `MarkAsReturned` throws `InvalidOperationException` when called twice, and leaves `ReturnDate`/`Status` unchanged after the failed second call

## 4. Coverage Verification

- [x] 4.1 Run `dotnet test --collect:"XPlat Code Coverage"` from the repo root
- [x] 4.2 Inspect the generated Cobertura report and confirm 100% line and branch coverage for `src/Domain/Entities/*.cs`
- [x] 4.3 Add any missing test cases needed to close coverage gaps, then re-run until 100% is reached
- [x] 4.4 Confirm the full suite (`dotnet test`) passes with no failing or skipped tests
