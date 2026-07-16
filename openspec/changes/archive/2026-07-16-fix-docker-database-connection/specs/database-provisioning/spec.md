## ADDED Requirements

### Requirement: MongoDB Container Exposes Its Actual Port
`src/docker-compose.yml`'s `db_read` service SHALL publish MongoDB's real listening port (`27017`) to the host, not an unrelated port.

#### Scenario: Connecting to MongoDB from the host after `docker compose up`
- **WHEN** the `db_read` container is running via `docker compose up`
- **THEN** a MongoDB client on the host can connect to `localhost:27017` and authenticate using the `DB_READ_USER`/`DB_READ_PASSWORD` credentials from `.env`

### Requirement: Environment-Layered Database Connection Strings
The system SHALL configure `ConnectionStrings:OpeaLibraryWriteConnection` and `ConnectionStrings:OpeaLibraryReadConnection` so that the base `appsettings.json` targets the Docker Compose service hostnames (`db_save`, `db_read`) for the API's future containerized deployment on `db_network`, while `appsettings.Development.json` overrides both to `localhost` (matching the host ports Compose publishes) for running the API as a host process during local development.

#### Scenario: Running the API locally via `dotnet run` or an IDE
- **WHEN** the API starts with no `ASPNETCORE_ENVIRONMENT` override (defaulting to `Development`)
- **THEN** it resolves `OpeaLibraryWriteConnection` and `OpeaLibraryReadConnection` to `localhost`-based connection strings and successfully connects to both databases published by `docker compose up`

#### Scenario: Base configuration targets the Compose network
- **WHEN** `appsettings.json` is read without a `Development` (or other environment-specific) override applied
- **THEN** `OpeaLibraryWriteConnection` and `OpeaLibraryReadConnection` resolve to hosts `db_save` and `db_read` respectively, matching the service names in `src/docker-compose.yml`

### Requirement: Postgres Schema Provisioning at Startup
The system SHALL apply pending EF Core migrations for `OpeaLibraryDbContext` at application startup in the `Development` environment, so `tb_books`/`tb_loans` and their constraints exist before the API serves requests — the same guarantee already provided for MongoDB's collections.

#### Scenario: Starting the API against a fresh Postgres database
- **WHEN** the API starts in the `Development` environment against a `db_save` container with no prior schema
- **THEN** `Database.Migrate()` runs before the app begins serving requests, and `tb_books`/`tb_loans` exist afterward with the columns, constraints, and the `Loan.BookId → Book` foreign key (`DeleteBehavior.Restrict`) defined by `BookConfig`/`LoanConfig`

#### Scenario: Starting the API against an already-migrated Postgres database
- **WHEN** the API starts in the `Development` environment against a `db_save` container whose schema is already up to date
- **THEN** `Database.Migrate()` completes without error and applies no changes

### Requirement: MongoDB Collection Provisioning at Startup
The system SHALL verify (and create if missing) the `books` and `loans` MongoDB collections at application startup in the `Development` environment, before the API serves requests.

#### Scenario: Starting the API against a fresh MongoDB database
- **WHEN** the API starts in the `Development` environment against a `db_read` container with no existing collections
- **THEN** `IMongoDatabaseInitializer.EnsureDatabaseCreatedAsync()` runs before the app begins serving requests, and the `books`/`loans` collections exist afterward

#### Scenario: Starting the API against an already-provisioned MongoDB database
- **WHEN** the API starts in the `Development` environment against a `db_read` container whose `books`/`loans` collections already exist
- **THEN** `EnsureDatabaseCreatedAsync()` completes without error and creates no duplicate collections
