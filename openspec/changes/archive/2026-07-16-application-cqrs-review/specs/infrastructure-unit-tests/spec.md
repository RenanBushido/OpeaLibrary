## MODIFIED Requirements

### Requirement: EF Core Configuration Coverage
The test suite SHALL verify that `BookConfig` and `LoanConfig` are applied to `OpeaLibraryDbContext`'s model with the expected table names, keys, property facets, and relationships.

#### Scenario: Book entity configuration is applied
- **WHEN** `OpeaLibraryDbContext.Model` is inspected for the `Book` entity type
- **THEN** it maps to table `tb_books`, has `Id` as its key, and marks `Title` (max length 200), `Author` (max length 100), `PublishedYear`, and `QuantityAvailable` as required

#### Scenario: Loan entity configuration is applied
- **WHEN** `OpeaLibraryDbContext.Model` is inspected for the `Loan` entity type
- **THEN** it maps to table `tb_loans`, has `Id` as its key, marks `BookId` and `LoanDate` as required, and marks `ReturnDate` as optional

#### Scenario: Loan entity has a foreign key relationship to Book
- **WHEN** `OpeaLibraryDbContext.Model` is inspected for the `Loan` entity type's foreign keys
- **THEN** it has exactly one foreign key on `BookId` referencing `Book`, with `DeleteBehavior.Restrict`
