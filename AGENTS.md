# AGENTS.md

## Build & Run

```bash
dotnet build LibraryAPIApp.sln
dotnet run --project LibraryAPIApp/LibraryAPIApp.csproj
```

Server starts at `https://localhost:5001`. Launch URL is `api/Test`.

## Test

```bash
dotnet test LibraryAPIApp.Tests/LibraryAPIApp.Tests.csproj
```

Run a single test class:
```bash
dotnet test LibraryAPIApp.Tests/LibraryAPIApp.Tests.csproj --filter "FullyQualifiedName~JwtTokenBuilderTests"
```

xUnit v3 with `Microsoft.Testing.Platform` runner (per `global.json`). No CI exists yet.

## Required SDK

.NET 10.0.400 (`global.json` with `rollForward: latestFeature`).

## Solution Structure

| Project | Purpose |
|---|---|
| `LibraryAPIApp/` | ASP.NET Core Web API entrypoint |
| `Common/` | Shared DTOs, models, config, utilities (no external packages) |
| `DataAccess/` | Data layer — Dapper queries + EF Core Identity entities |
| `LibraryAPIApp.Tests/` | Unit tests (xUnit v3 + Moq); functional test infrastructure referenced but not yet built |

Dependency chain: `LibraryAPIApp → DataAccess → Common`.

## Configuration

`appsettings.json` is empty `{}`. All secrets go through **User Secrets** (id: `a51bb98d-11ba-4000-977f-5e7d0472fdad`). Required keys:

- `ConnectionStrings:DefaultConnection` — MariaDB/MySQL connection string
- `ApiConfiguration:RDBMS` — set to `"MySQL"`
- `Jwt:SecretKey` — HS256 signing key
- `DefaultAdmins:Password` — admin seed password

Fallbacks exist for Jwt issuer/audience and company/branch seed values.

## Database

- **Engine:** MariaDB (MySQL dialect) via `MySqlConnector` + `MySql.EntityFrameworkCore`
- **Identity:** EF Core (`EnsureCreated`), no migration history
- **Business tables:** Created from `LibraryAPIApp/Data/Schema/Tables.txt` DDL (run manually or via DB init)
- **Schema upgrades:** `Program.cs` runs idempotent raw SQL for legacy `Company`/`Branch`/`LastUserCompanyAndBranch` columns/tables
- **ORM split:** Dapper for business table queries, EF Core only for Identity and the Company/Branch/LUCB entities

## Key Conventions

- MediatR 14.2.0 — controllers delegate through `ApiControllerBase.QueryAsync`/`CommandAsync`
- JWT Bearer auth with role policies: `Administrator`, `Librarian`, `User` (defined in `Policies.cs`)
- Data access is DI-injected; `RepositoryBase.OpenConnection()` switches between MySQL and SqlServer based on `ApiConfiguration:RDBMS`
- `Common/Util/RijndaelCrypt.cs` uses a legacy-compatible AES/MD5 implementation — pinned test vector: `"Hello"` → `"Fu8uODSCDuk6tCZv5xE9Bw=="`

## Known Issues

- `GET api/User/UserInfo` (`UserController`) has a known 500 error
- `LibraryAPIApp.Tests/` references `Microsoft.AspNetCore.Mvc.Testing` and `Testcontainers.MariaDb` but the `Functional/` test suite does not exist yet
- Stale Dependabot PRs exist on remote for pre-upgrade package versions

## Git

Current branch: `Net10Conversion` (tracks origin). Clean working tree.
