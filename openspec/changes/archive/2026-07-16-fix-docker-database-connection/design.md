## Context

`src/docker-compose.yml` defines two database containers (`db_save` for Postgres, `db_read` for MongoDB) but no `api` service — the API currently runs as a host process (`dotnet run`/IDE), not inside the Compose network, and is planned to be containerized later. `Program.cs` already has one working example of startup schema provisioning: `IMongoDatabaseInitializer.EnsureDatabaseCreatedAsync()`, called inside `if (app.Environment.IsDevelopment())`, which lists Mongo collections and creates `books`/`loans` if missing. Postgres has no equivalent — `src/Infrastructure` has no `Migrations/` folder, `AddInfraPostgres` only calls `AddDbContext`, and nothing calls `Database.Migrate()` or `EnsureCreated()`. Combined with `db_read`'s host port being published as `8081` instead of Mongo's real `27017`, and the Mongo connection string hardcoding host `mongo` (unresolvable both from the host machine and from any container, since no container is currently named `mongo` on `db_network`), the API cannot reach either database today, and even a reachable Postgres would have no tables.

## Goals / Non-Goals

**Goals:**
- Make both databases reachable when running the API locally today (as a host process against Dockerized databases).
- Keep the connection-string configuration correct for the already-planned future where the API itself joins `db_network` as a container.
- Give Postgres the same "schema exists before serving requests" guarantee Mongo already has, using EF Core migrations (the intended mechanism — `Microsoft.EntityFrameworkCore.Design`/`.Tools` are already referenced in `src/Infrastructure`).

**Non-Goals:**
- Containerizing the API itself (adding an `api` service to `docker-compose.yml`, a `Dockerfile`) — explicitly deferred by the user to a later change.
- A production-grade migration deployment strategy (e.g., running migrations as a separate release step/job rather than at app startup). This change mirrors the existing `IsDevelopment()`-gated pattern already used for Mongo; production migration strategy is a separate concern.
- Any change to `src/Application`, `src/Domain`, or the CQRS read/write repository contracts — this change only touches configuration, `Program.cs` startup wiring, and adds the first EF Core migration.

## Decisions

### 1. Split `ConnectionStrings` between `appsettings.json` and `appsettings.Development.json` by target environment, rather than picking one hostname scheme
`appsettings.json` (the base config — the one that will still apply once the API is containerized and no `appsettings.Development.json` override is present, e.g. in a `Production` container) sets both connection strings to the Compose service names (`db_save`, `db_read`), which is what resolves once the API joins `db_network`. `appsettings.Development.json` (which ASP.NET Core layers on top of the base file when `ASPNETCORE_ENVIRONMENT=Development` — the default when running via `dotnet run`/an IDE with no environment variable set) overrides both to `localhost`, matching the ports Compose publishes to the host. This directly serves the user's stated intent (API not containerized yet, but will be) without having to guess a single "right now" answer that would work against the stated future plan.
- **Alternative considered**: Put `localhost` in the base `appsettings.json` and override with service names in a new `appsettings.Docker.json` (or similar) activated only once the API is containerized. Rejected for now — it adds a new environment/config file for a container setup that doesn't exist yet in this change; simpler to let the already-present `Development` layering carry today's need, and revisit the environment split when the `api` service itself is actually added.

### 2. Fix `db_read`'s published port (`8081` → `27017`)
MongoDB listens on `27017` by default; `8081` is not a port `mongo:latest` exposes for anything (it's commonly associated with `mongo-express`, which isn't a service in this compose file). Whatever the intended connection string host resolves to, the container was never reachable on the database's actual port. This is a plain typo/copy-paste fix, not a design choice.

### 3. Add the first EF Core migration and call `Database.Migrate()` alongside the existing Mongo initializer
`OpeaLibraryDbContext` (via `BookConfig`/`LoanConfig`) fully describes the intended schema (`tb_books`, `tb_loans`, the `Loan.BookId → Book` FK with `DeleteBehavior.Restrict` added by a prior change) but nothing has ever generated a migration from it. `dotnet ef migrations add InitialCreate` (run with `--project src/Infrastructure --startup-project src/Presentation`) generates the first migration from the current model; `Program.cs` then calls `dbContext.Database.Migrate()` inside the same `if (app.Environment.IsDevelopment())` block that already calls `IMongoDatabaseInitializer.EnsureDatabaseCreatedAsync()`, applying it (and any future migration) automatically on startup — the direct Postgres analogue of what the Mongo initializer already does for collections.
- **Alternative considered**: `Database.EnsureCreated()`. Rejected per explicit user decision — `EnsureCreated()` doesn't use the migrations pipeline at all (no `__EFMigrationsHistory` table), and is documented by Microsoft as incompatible with ever calling `Migrate()` later on the same database; since `Microsoft.EntityFrameworkCore.Design`/`.Tools` are already referenced (clearly anticipating migrations), starting on `EnsureCreated()` would just create migration-adoption pain for the first real schema change.

## Risks / Trade-offs

- **[Risk]** `Database.Migrate()` running at every app startup (mirroring the existing Mongo pattern) means schema changes ship coupled to app deploys, with no separate review/rollback step — acceptable for local development (the only environment this is gated to, via `IsDevelopment()`), but not a production migration strategy. → **Mitigation**: Explicitly out of scope (see Non-Goals); flagged here so it isn't mistaken for a deliberate production-readiness decision.
- **[Risk]** The base `appsettings.json` now points at hostnames (`db_save`, `db_read`) that don't resolve for anyone running the API outside `Development` and outside the Compose network (e.g., `dotnet run --environment Production` on a host machine) — it would fail to connect. → **Mitigation**: Acceptable today because the only two ways this app currently runs are (a) `Development` on the host (covered by the new `appsettings.Development.json` override) or (b) not at all in any other environment yet, since no container/deployment path exists for the API. Revisit when the API is actually containerized.
- **[Risk]** Generating `InitialCreate` now, before the API has ever run against a schema-bearing Postgres, means this migration has no production data to consider — low risk, but worth confirming the generated migration matches `BookConfig`/`LoanConfig` exactly (table names `tb_books`/`tb_loans`, the `Loan → Book` FK with `Restrict`) before applying it, rather than trusting codegen blindly. → **Mitigation**: `tasks.md` includes an explicit review step comparing the generated migration's `Up()` against the EF configurations.

## Migration Plan

1. Fix `src/docker-compose.yml`'s port mapping first — cheapest, independently verifiable (`docker compose up`, confirm Mongo is reachable on `27017`).
2. Update `appsettings.json`/`appsettings.Development.json` connection strings.
3. Generate and review the `InitialCreate` EF Core migration; do not apply it manually — let the new `Database.Migrate()` startup call apply it, so the exact code path being shipped is the one that gets tested.
4. Add the `Database.Migrate()` call in `Program.cs` and run the API locally end-to-end against `docker compose up` to confirm both databases connect and the write side actually persists (a real `AddBookCommand` round-trip), not just that the app boots.

No rollback beyond normal git revert — this only affects local development configuration and adds new, non-destructive schema (an empty Postgres database gaining its first two tables).

## Open Questions

None outstanding — the two ambiguous points (API's current vs. future runtime location; `Migrate()` vs. `EnsureCreated()`) were resolved directly with the user before this design was written.
