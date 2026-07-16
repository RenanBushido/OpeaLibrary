## ADDED Requirements

### Requirement: xUnit Test Project for CrossCutting Layer
The system SHALL provide an xUnit test project at `tests/OpeaLibrary.CrossCutting.Tests/` that references `OpeaLibrary.CrossCutting` (at `src/CrossCutting/OpeaLibrary.CrossCutting.csproj`) and is runnable via `dotnet test`, using an in-test `IConfiguration` with placeholder connection strings so no external Postgres or MongoDB connection is required to verify service registration.

#### Scenario: Running the test suite
- **WHEN** a developer runs `dotnet test` against `tests/OpeaLibrary.CrossCutting.Tests/OpeaLibrary.CrossCutting.Tests.csproj`
- **THEN** all CrossCutting unit tests execute and report pass/fail results using xUnit, with no external database connection required

### Requirement: ApplicationExtensions Registration Coverage
The test suite SHALL verify that `ApplicationExtensions.AddApiMediatR`, `AddApiAutoMapper`, and `AddApiValidators` each register their expected services on an `IServiceCollection`.

#### Scenario: AddApiMediatR registers MediatR handlers and the validation pipeline behavior
- **WHEN** `AddApiMediatR` is called on an `IServiceCollection`
- **THEN** the resulting `IServiceCollection` contains registrations resolving `IMediator`, and includes `ValidationBehavior<,>` as a registered open pipeline behavior

#### Scenario: AddApiAutoMapper registers the MappingProfile
- **WHEN** `AddApiAutoMapper` is called on an `IServiceCollection` and the collection is built into a `ServiceProvider`
- **THEN** an `IMapper` can be resolved and its configuration includes the maps declared in `MappingProfile`

#### Scenario: AddApiValidators registers all four validators
- **WHEN** `AddApiValidators` is called on an `IServiceCollection` and the collection is built into a `ServiceProvider`
- **THEN** `IValidator<AddBookCommand>`, `IValidator<GetBookByIdRequest>`, `IValidator<RequestLoanCommand>`, and `IValidator<ReturnLoanCommand>` each resolve to their expected validator implementation with a scoped lifetime

### Requirement: InfrastructureExtensions Registration Coverage
The test suite SHALL verify that `InfrastructureExtensions.AddInfraPostgres` and `AddInfraMongo` each register their expected services on an `IServiceCollection` given a valid in-test `IConfiguration`, and that each throws `InvalidOperationException` when its required connection string is missing.

#### Scenario: AddInfraPostgres registers the write repositories and unit of work
- **WHEN** `AddInfraPostgres` is called with an `IConfiguration` containing a valid `OpeaLibraryWriteConnection` connection string
- **THEN** the resulting `IServiceCollection` contains scoped registrations resolving `IBookWriteRepository` to `BookWriteRepository`, `ILoanWriteRepository` to `LoanWriteRepository`, and `IUnitOfWork` to `UnitOfWork`

#### Scenario: AddInfraPostgres throws when the write connection string is missing
- **WHEN** `AddInfraPostgres` is called with an `IConfiguration` that has no `OpeaLibraryWriteConnection` entry
- **THEN** an `InvalidOperationException` is thrown

#### Scenario: AddInfraMongo registers the read repositories
- **WHEN** `AddInfraMongo` is called with an `IConfiguration` containing a valid `OpeaLibraryReadConnection` connection string
- **THEN** the resulting `IServiceCollection` contains scoped registrations resolving `IBookReadRepository` to `BookReadRepository`, `ILoanReadRepository` to `LoanReadRepository`, and `IMongoDatabaseInitializer` to `MongoDatabaseInitializer`

#### Scenario: AddInfraMongo throws when the read connection string is missing
- **WHEN** `AddInfraMongo` is called with an `IConfiguration` that has no `OpeaLibraryReadConnection` entry
- **THEN** an `InvalidOperationException` is thrown

### Requirement: 100% Code Coverage of CrossCutting Layer
The test suite SHALL achieve 100% line and branch coverage of all types under `src/CrossCutting/`, verifiable via `dotnet test --collect:"XPlat Code Coverage"`, with one documented exception: the bodies of the `IDbConnection`/`IMongoClient` factory delegates inside `AddInfraPostgres`/`AddInfraMongo` (which open a real `NpgsqlConnection` and construct a real `MongoClient`) are excluded, since they only execute when actually resolved from a built `ServiceProvider` — which this suite deliberately avoids doing for `IDbConnection`/`IMongoClient` specifically, to keep the CrossCutting test project free of any live Postgres/MongoDB connection requirement (per the `xUnit Test Project for CrossCutting Layer` requirement above).

#### Scenario: Verifying coverage after running tests
- **WHEN** `dotnet test --collect:"XPlat Code Coverage"` is run against the CrossCutting test project
- **THEN** the generated Cobertura coverage report shows 100% line coverage and 100% branch coverage for `src/CrossCutting/**/*.cs`, except for the documented factory-delegate exception above
