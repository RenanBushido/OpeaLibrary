## ADDED Requirements

### Requirement: Entity Domain Event Collection
`Entity` SHALL provide a mechanism for subclasses to record domain events (`AddDomainEvent`), expose them for inspection (`DomainEvents`, read-only), and clear them (`ClearDomainEvents`).

#### Scenario: Adding a domain event
- **WHEN** a subclass calls `AddDomainEvent` with an `IDomainEvent`
- **THEN** that event appears in the entity's `DomainEvents` collection

#### Scenario: Clearing domain events
- **WHEN** `ClearDomainEvents` is called on an entity with one or more recorded events
- **THEN** `DomainEvents` is empty afterward

### Requirement: Book Entity Raises Domain Events
`Book.Create`, `Book.DecreaseQuantity`, and `Book.IncreaseQuantity` SHALL each add the appropriate domain event to the created/mutated `Book`'s `DomainEvents` collection.

#### Scenario: Creating a book raises BookCreatedEvent
- **WHEN** `Book.Create` successfully creates a `Book`
- **THEN** the returned `Book`'s `DomainEvents` contains a `BookCreatedEvent` with that book's `Id`, `Title`, `Author`, `PublishedYear`, and `QuantityAvailable`

#### Scenario: Decreasing quantity raises BookQuantityChangedEvent
- **WHEN** `DecreaseQuantity` is called successfully on a `Book`
- **THEN** the book's `DomainEvents` contains a `BookQuantityChangedEvent` with that book's `Id` and the new `QuantityAvailable`

#### Scenario: Increasing quantity raises BookQuantityChangedEvent
- **WHEN** `IncreaseQuantity` is called on a `Book`
- **THEN** the book's `DomainEvents` contains a `BookQuantityChangedEvent` with that book's `Id` and the new `QuantityAvailable`

#### Scenario: A failed mutation raises no event
- **WHEN** `DecreaseQuantity` is called on a `Book` whose `QuantityAvailable` is 0 (and throws)
- **THEN** no `BookQuantityChangedEvent` is added to the book's `DomainEvents`

### Requirement: Loan Entity Raises Domain Events
`Loan.Create` and `Loan.MarkAsReturned` SHALL each add the appropriate domain event to the created/mutated `Loan`'s `DomainEvents` collection.

#### Scenario: Creating a loan raises LoanCreatedEvent
- **WHEN** `Loan.Create` successfully creates a `Loan`
- **THEN** the returned `Loan`'s `DomainEvents` contains a `LoanCreatedEvent` with that loan's `Id`, `BookId`, and `LoanDate`

#### Scenario: Marking a loan as returned raises LoanReturnedEvent
- **WHEN** `MarkAsReturned` is called successfully on a `Loan`
- **THEN** the loan's `DomainEvents` contains a `LoanReturnedEvent` with that loan's `Id` and `ReturnDate`

#### Scenario: A failed return raises no event
- **WHEN** `MarkAsReturned` is called on a `Loan` that is already returned (and throws)
- **THEN** no additional `LoanReturnedEvent` is added to the loan's `DomainEvents`

## MODIFIED Requirements

### Requirement: Book Entity Coverage
The test suite SHALL exercise every code path of `Book.Create`, `Book.Restore`, `Book.DecreaseQuantity`, and `Book.IncreaseQuantity`.

#### Scenario: Creating a book with valid data
- **WHEN** `Book.Create` is called with a non-empty title, non-empty author, and a positive published year
- **THEN** a `Book` is returned with a non-empty `Id`, the given `Title`, `Author`, `PublishedYear`, and `QuantityAvailable` equal to 0

#### Scenario: Creating a book with an empty or whitespace title
- **WHEN** `Book.Create` is called with a `title` that is null, empty, or whitespace
- **THEN** a `DomainException` is thrown referencing that the title cannot be empty

#### Scenario: Creating a book with an empty or whitespace author
- **WHEN** `Book.Create` is called with an `author` that is null, empty, or whitespace
- **THEN** a `DomainException` is thrown referencing that the author cannot be empty

#### Scenario: Creating a book with a non-positive published year
- **WHEN** `Book.Create` is called with `publishedYear` less than or equal to 0
- **THEN** a `DomainException` is thrown referencing that the published year must be a positive integer

#### Scenario: Restoring a book from persisted state
- **WHEN** `Book.Restore` is called with an `id`, `title`, `author`, `publishedYear`, and `quantityAvailable`
- **THEN** a `Book` is returned with exactly those values (no new `Id` is generated), and no domain event is raised

#### Scenario: Decreasing quantity when copies are available
- **WHEN** `DecreaseQuantity` is called on a `Book` whose `QuantityAvailable` is greater than 0
- **THEN** `QuantityAvailable` is decremented by 1

#### Scenario: Decreasing quantity when no copies are available
- **WHEN** `DecreaseQuantity` is called on a `Book` whose `QuantityAvailable` is 0
- **THEN** a `DomainException` is thrown and `QuantityAvailable` remains unchanged

#### Scenario: Increasing quantity
- **WHEN** `IncreaseQuantity` is called on a `Book`
- **THEN** `QuantityAvailable` is incremented by 1

### Requirement: 100% Code Coverage of Domain Layer
The test suite SHALL achieve 100% line and branch coverage of all types under `src/Domain/Entities/`, `src/Domain/Enums/`, and `src/Domain/Events/`, verifiable via `dotnet test --collect:"XPlat Code Coverage"`.

#### Scenario: Verifying coverage after running tests
- **WHEN** `dotnet test --collect:"XPlat Code Coverage"` is run against the Domain test project
- **THEN** the generated Cobertura coverage report shows 100% line coverage and 100% branch coverage for `src/Domain/Entities/*.cs` and `src/Domain/Events/*.cs`
