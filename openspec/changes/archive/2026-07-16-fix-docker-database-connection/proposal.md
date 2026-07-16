## Why

The API can't reach its databases when run today. Three independent bugs compound the failure: `src/docker-compose.yml` publishes MongoDB's container on host port `8081` instead of its actual `27017`, so nothing on the host can ever reach it regardless of hostname; `appsettings.json`/`appsettings.Development.json` point the Mongo connection string at host `mongo` (not a name Compose resolves for either the host or, currently, any other container, since no `api` service exists yet on `db_network`); and — the deeper issue behind "valida se existem tabelas e banco de dados criados antes de subir a API" — only the MongoDB side has that startup provisioning (`IMongoDatabaseInitializer.EnsureDatabaseCreatedAsync()` in `Program.cs`); the Postgres side has no EF Core migrations at all (`src/Infrastructure` has no `Migrations/` folder) and nothing ever calls `Database.Migrate()`, so `tb_books`/`tb_loans` are never created — every write-side call fails even once connectivity itself is fixed.

## What Changes

- Fix `src/docker-compose.yml`'s `db_read` service: publish MongoDB's actual port `27017:27017` instead of the current `8081:8081` (which matches no process MongoDB runs), so the database is reachable from the host during local development.
- Split the two `ConnectionStrings` between `appsettings.json` and `appsettings.Development.json` by intent, since the API isn't containerized yet but will be later: `appsettings.json` (the base config, meant for the API's eventual containerized deployment on `db_network`) uses the Compose service names `db_save`/`db_read` as hosts; `appsettings.Development.json` (which layers on top when running locally via `dotnet run`/an IDE, outside Docker) overrides both to `localhost`, matching the ports Compose publishes to the host. This unblocks local development now without working against the planned future containerization.
- Add the missing EF Core migration for `OpeaLibraryDbContext` (`Book`/`Loan`, per `BookConfig`/`LoanConfig`) and call `dbContext.Database.Migrate()` in `Program.cs`, mirroring the existing `IMongoDatabaseInitializer.EnsureDatabaseCreatedAsync()` call (same `IsDevelopment()` block) so the Postgres write side gets the same "ensure schema exists before serving requests" guarantee the Mongo read side already has.

## Capabilities

### New Capabilities
- `database-provisioning`: The API's startup-time guarantee that both databases (Postgres write side, MongoDB read side) are reachable and have their required schema/collections before the app starts serving requests, plus the local Docker Compose/`appsettings` configuration that makes those databases reachable in the first place.

### Modified Capabilities
(none — no existing spec covers Presentation/Program.cs startup behavior or docker-compose configuration; `application-unit-tests`, `domain-unit-tests`, and `infrastructure-unit-tests` are unaffected, as this change touches no test suite and no `src/Application`/`src/Domain` code)

## Impact

- **Affected code**: `src/docker-compose.yml` (port mapping fix), `src/Presentation/appsettings.json` and `appsettings.Development.json` (connection string host split), `src/Presentation/Program.cs` (new `Database.Migrate()` call alongside the existing Mongo initializer call).
- **New code**: `src/Infrastructure/Migrations/` — the initial EF Core migration for `OpeaLibraryDbContext`.
- **Risk**: Low — these are local-development connectivity/config fixes and an additive schema-provisioning call; no application request-handling logic changes. The migration is new and additive (no existing production data/deployment depends on the current schema-less state, since nothing could persist to Postgres before this fix). `Database.Migrate()` running at every startup (mirroring the existing Mongo pattern) is a `Development`-only convenience for now, not a production deployment strategy — noted as a scoping decision in `design.md`, not solved here.
