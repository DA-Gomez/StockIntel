# StockIntel — Project Context

> A backend API for a stock-watchlist application. Users register, log in, create
> watchlists, and add stock tickers to them. Built in **.NET 8** following
> **Clean Architecture** with a lightweight **CQRS** (command/query + handler) pattern.
>
> This file is a high-level orientation document for an AI assistant. It summarizes
> the architecture, conventions, and current state so you can reason about the code
> without re-reading every file.

---

## 1. What the project is

StockIntel is a REST API (no frontend in this repo yet). The functional surface today:

- **Users**: register (email + password), log in (returns a JWT).
- **Watchlists**: create a watchlist, list your watchlists, add a ticker to a watchlist.
- Authorization is per-user: you can only touch watchlists you own.

It is an early-stage / learning-grade project — the README and source contain
explanatory inline comments. "Stage 1" was recently completed (see git log), and the
app is being prepared for deployment to **Fly.io** (`fly.toml` present, region `yyz`,
512 MB / 1 CPU).

---

## 2. Tech stack

| Concern            | Choice |
|--------------------|--------|
| Language / runtime | C# / .NET 8 (`net8.0`, nullable + implicit usings enabled) |
| Web framework      | ASP.NET Core (controllers, not minimal APIs) |
| Persistence        | EF Core 8 + **PostgreSQL** (Npgsql), snake_case naming convention |
| Auth               | JWT bearer tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Password hashing   | BCrypt (`BCryptPasswordHasher`) |
| Validation         | FluentValidation |
| API docs           | Swagger / Swashbuckle (dev only, at `/swagger`) |
| Containerization   | Docker (multi-stage, alpine), docker-compose for local dev |
| Tests              | xUnit + Testcontainers (real Postgres in a container) |
| CI                 | GitHub Actions (build → test → docker build) |
| Deployment target  | Fly.io |

---

## 3. Solution layout (Clean Architecture)

Solution file: `StockIntel.sln`. Four production projects under `src/`, three test
projects under `tests/`. Dependency direction points **inward** (API → Infrastructure
→ Application → Domain; Domain depends on nothing).

```
src/
  StockIntel.Domain/          # Entities, value objects. Pure C#, no dependencies.
  StockIntel.Application/      # Use cases (commands/queries + handlers), abstractions (interfaces).
  StockIntel.Infrastructure/   # EF Core, repositories, JWT generation, password hashing — implements Application interfaces.
  StockIntel.Api/              # ASP.NET controllers, DI composition root, Program.cs, auth pipeline.
tests/
  StockIntel.Domain.UnitTests/        # Entity invariants (e.g. User).
  StockIntel.Application.UnitTests/    # Handlers, value objects (Ticker), entity behavior (Watchlist).
  StockIntel.Api.IntegrationTests/     # Full HTTP tests against a real Postgres via Testcontainers.
```

### Layer responsibilities

- **Domain** — `User`, `Watchlist`, `WatchlistItem` entities and the `Ticker` value object.
  Entities have **private setters** and **static factory methods** (`User.Register(...)`,
  `Watchlist.Create(...)`, `Ticker.Create(...)`) that enforce invariants. Private
  parameterless constructors exist only for EF Core. No business rule is enforced
  outside the domain.

- **Application** — One folder per use case (e.g. `Users/Register/`, `Watchlists/AddTicker/`),
  each containing a `Command`/`Query` record + a `Handler`. Depends on **abstractions**
  (`Abstractions/Persistence`, `Abstractions/Security`, `Abstractions/Authentication`)
  that Infrastructure implements. Knows nothing about HTTP or EF Core.

- **Infrastructure** — `AppDbContext`, entity `Configurations`, `Repositories`,
  `UnitOfWork`, `JwtTokenGenerator`, `BCryptPasswordHasher`, EF migrations. Wires
  itself up in `Infrastructure/DependencyInjection.cs` (`AddInfrastructure`).

- **Api** — Thin controllers that translate HTTP ↔ commands/queries and map domain
  exceptions to status codes. `Program.cs` is the composition root.

---

## 4. CQRS-lite pattern (important convention)

There is no MediatR. A hand-rolled, explicit handler abstraction is used instead,
defined in `Application/Common/`:

```csharp
public interface ICommand<TResponse> { }
public interface ICommand : ICommand<Unit> { }            // Unit = "no meaningful return"

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken ct);
}
// IQuery<TResponse> / IQueryHandler<...> mirror this for reads.
```

**Adding a new use case** means:
1. Create a folder `Application/<Area>/<UseCase>/`.
2. Add a `record` command/query implementing `ICommand<T>` / `IQuery<T>`.
3. Add a handler implementing `ICommandHandler<,>` / `IQueryHandler<,>`.
4. (Optional) add a FluentValidation validator.
5. **Register it** in `Application/DependencyInjection.cs` (`AddApplication`).
6. Add/extend a controller action in `Api/Controllers/` that builds the command and
   calls `HandleAsync`.

Controllers inject the handler interface directly (constructor injection); they do
**not** inject repositories or DbContext.

---

## 5. Request flow (example: add a ticker)

```
HTTP POST /api/watchlists/{id}/tickers  [Authorize]
  -> WatchlistController.AddTicker
       builds AddTickerCommand(watchlistId, symbol)
  -> AddTickerHandler.HandleAsync
       reads ICurrentUser.UserId (from JWT)        -> 401 if null
       loads watchlist via IWatchlistRepository    -> KeyNotFoundException -> 404
       checks watchlist.UserId == userId           -> UnauthorizedAccessException -> 403
       Ticker.Create(symbol)                        -> ArgumentException -> 400
       watchlist.AddTicker(ticker)                  (domain dedupes)
       IUnitOfWork.SaveChangesAsync()
```

**Error-handling convention**: handlers/domain throw exceptions; controllers
`try/catch` and map them to HTTP status codes. Current mappings seen in the code:

- `ValidationException` (FluentValidation) → **400**
- `ArgumentException` (domain invariant) → **400**
- `InvalidOperationException` (e.g. duplicate email) → **409**
- `KeyNotFoundException` → **404**
- `UnauthorizedAccessException` → **401** (login) / **403** (ownership)

---

## 6. Authentication

- Login issues a JWT via `IJwtTokenGenerator` / `JwtTokenGenerator`, configured from
  the `Jwt` config section (`Issuer`, `Audience`, `SigningKey`, `ExpirationMinutes`).
- `Program.cs` sets up JWT bearer validation: validates issuer, audience, lifetime,
  signing key; `ClockSkew = Zero`; `MapInboundClaims = false` (keeps raw `sub`/`email`
  claim names).
- `ICurrentUser` (implemented by `Api/Authentication/CurrentUser.cs`) reads the
  current user's id from the JWT claims via `IHttpContextAccessor`. Handlers depend on
  this abstraction, not on HTTP.
- `[Authorize]` is applied at the `WatchlistController` level; `UsersController`
  (register/login) is anonymous.

---

## 7. Data / persistence

- `AppDbContext` + per-entity `IEntityTypeConfiguration` classes in
  `Infrastructure/Persistence/Configurations/`.
- **Snake_case** column/table naming (`UseSnakeCaseNamingConvention`).
- Repositories: `UserRepository`, `WatchlistRepository`; `UnitOfWork` wraps
  `SaveChangesAsync`. Registered as **Scoped** (one per request); `IPasswordHasher`
  and `IJwtTokenGenerator` are **Singleton**.
- Migrations live in `Infrastructure/Migrations/` (initial: `InitialSchema`,
  dated 2026-05-27). **Migrations are applied automatically on startup** in
  `Program.cs` via `db.Database.MigrateAsync()`.

---

## 8. Local development

Prereqs: Docker Desktop, .NET 8 SDK, Git.

```bash
# Everything in containers (api + postgres):
docker compose up --build       # --build only when source changed
# API + Swagger at http://localhost:8080/swagger

# Or run Postgres in a container and the API on the host:
docker compose up -d            # postgres only (detached)
dotnet run --project src/StockIntel.Api

# EF migrations:
dotnet ef migrations add <Name> --project src/StockIntel.Infrastructure --startup-project src/StockIntel.Api
dotnet ef database update      --project src/StockIntel.Infrastructure --startup-project src/StockIntel.Api

# Connect to local Postgres:
docker exec -it stockintel-postgres psql -U stockintel -d stockintel   # \dt  \q
```

`docker-compose.yml` defines two services: `postgres` (postgres:16-alpine, healthcheck,
named volume `postgres_data` so data survives container removal) and `api` (built from
the Dockerfile, waits for postgres to be healthy). Local dev config (connection string,
JWT settings) is injected via environment variables using the ASP.NET `__` nesting
convention (`ConnectionStrings__Postgres`, `Jwt__SigningKey`, etc.).

---

## 9. Testing

- **Unit tests** (`Domain.UnitTests`, `Application.UnitTests`): entity invariants,
  `Ticker`/`Watchlist` behavior, handler logic.
- **Integration tests** (`Api.IntegrationTests`): spin up a **real Postgres** in a
  Testcontainers container (`PostgresFixture`, xUnit collection fixture), run
  migrations, and exercise endpoints through `WebApplicationFactory<Program>`
  (`StockIntelApiFactory` swaps in the test connection string).
- `Program.cs` ends with `public partial class Program { }` specifically so the
  integration test factory can reference the entry point.
- Run all: `dotnet test`.

---

## 10. CI/CD

- **GitHub Actions** (`.github/workflows/ci.yml`), triggers on push/PR to `main`:
  1. `test` job — restore, build (Release), `dotnet test` on Ubuntu (Testcontainers
     runs Postgres automatically).
  2. `docker` job — builds the Docker image (no push), only if `test` passed; uses
     GHA build cache.
- **Dockerfile** — two-stage (sdk-alpine build → aspnet-alpine runtime), copies
  `.csproj` files first for layer-cached `restore`, publishes Release, runs as the
  non-root `app` user, exposes `8080`, `ASPNETCORE_ENVIRONMENT=Production`.
- **Deployment** — `fly.toml` targets Fly.io app `stock-intel` (region `yyz`,
  internal port 8080, force HTTPS, auto stop/start machines, 512 MB / 1 CPU).

---

## 11. Conventions & gotchas for an assistant

- **Domain stays pure**: enforce new rules in entity factory methods / value objects,
  not in handlers or controllers.
- **Every new use case must be registered** in `Application/DependencyInjection.cs`,
  or DI resolution fails at runtime.
- **Controllers are thin**: build a command/query, call the handler, map exceptions to
  status codes. Don't put business logic or data access in controllers.
- **Repositories return domain entities**; persistence shape (snake_case, EF configs)
  stays in Infrastructure.
- Production HTTPS redirect is on only outside Development; Swagger is Development-only.
- There are leftover scaffolding files (`Class1.cs`, `UnitTest1.cs`) and `.csproj.lscache`
  files — these are not meaningful and can be ignored / cleaned up.
- Some inline comments and the README contain minor typos; treat the code as source of
  truth over comments.

---

## 12. Current state (as of this snapshot)

- "Stage 1" complete: working register/login + watchlist create/list/add-ticker, with
  JWT auth, EF Core + Postgres, Docker, CI, and a test suite.
- In-progress / next: Fly.io deployment (`fly.toml` is new and untracked; `.dockerignore`
  has local modifications).
- **Not yet present**: remove-ticker / delete-watchlist endpoints (domain has
  `RemoveTicker` but no exposed endpoint), any real stock-market data integration
  (the name "StockIntel" implies future market/price features that don't exist yet),
  and any frontend.
