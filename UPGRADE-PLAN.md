# Upgrade Plan: LibraryAPI from .NET Core 3.0 to .NET 10

> **Date:** 2026-09-13
> **Target:** .NET 10 (SDK 10.0.400, installed locally)
> **Status:** ✅ IMPLEMENTED & VERIFIED (2026-09-13) — see "As-Built" below.

---

## 0. As-Built Summary (post-implementation)

The upgrade described below has been **completed and verified live** against MariaDB 12.3.3.

### What was actually done
- **All 3 projects now target `net10.0`** (`LibraryAPIApp`, `Common`, `DataAccess`). `netstandard2.1` dropped.
- **MySQL provider switched to Oracle `MySql.EntityFrameworkCore` 10.0.9** (Pomelo has no EF Core 10 release). EF Core 10.0.12.
- Packages upgraded: MediatR 7→14.2.0, JWT 8.22.0, Dapper 2.1.86, MySqlConnector 2.6.2 (namespace `MySqlConnector`, the legacy `MySql.Data.MySqlClient` shim is gone), SqlClient 7.0.3, Identity 10.0.12.
- `Newtonsoft.Json`, `MediatR.Extensions.Microsoft.DependencyInjection`, `System.Configuration.ConfigurationManager`, `System.ComponentModel.Annotations` all **removed**.
- `Program.cs` rewritten with the **minimal hosting model** (`WebApplication.CreateBuilder`); `Startup.cs` deleted.
- **`ServiceLocator` removed**; `IdentityDb` refactored to constructor-injected `DbContextOptions<IdentityDb>` (no `OnConfiguring`, no parameterless ctor, no `IConfiguration` resolution).
- **Middleware order fixed** — `UseAuthentication()` now runs before `UseAuthorization()`.
- JWT key/issuer/audience are config-driven (`Jwt:Issuer/Audience/SecretKey`).
- `JwtSecurityKey.Create` now **derives the HS256 key via SHA-256** of the secret (fixes JWT v8 "key must be greater than 256 bits" — the old 17-char secret is 136 bits).
- Default admin password is config-driven (`DefaultAdmins:Password`).
- Replaced **obsolete crypto APIs** (0 warnings now): `RNGCryptoServiceProvider` → `RandomNumberGenerator.Fill` (`RandomKeyGenerator`); `MD5CryptoServiceProvider`/`RijndaelManaged` → `MD5.Create`/`Aes.Create` (`RijndaelCrypt`, byte-compatible so existing encrypted data still decrypts).
- Deleted stale/corrupt EF Core 3.1 `Migrations/`, dead `AppConfig .cs`/`IAppConfig.cs`, `DbContextExtension.cs`, `ServiceLocator.cs`, and the template `WeatherForecast*` files.
- Fixed pre-existing CS0104 ambiguity (`System.Data.SqlClient` + `Microsoft.Data.SqlClient` both imported) in `RepositoryBase.cs`.
- Added `global.json` pinning SDK 10.0.400; `ImplicitUsings` enabled on all projects.

### 🔑 Test credentials (test app only — you asked me to save these)
| Field | Value |
|---|---|
| Username | `za1012001@yahoo.co.uk` |
| Password | `Password123@` |
| Login endpoint | `POST https://localhost:5001/api/Token` (JSON body: `{"username":"…","password":"…"}`) |
| Auth header | `Authorization: Bearer <token>` |

> Default admin emails come from `DefaultAdmins:DbAdmins` (User Secrets). If `DefaultAdmins:Password` is not set, the fallback is `Password123@`, and seeding only runs the first time (`context.Users.Any()` gates it).

### Verified live (2026-09-13)
- `POST api/Token` with the credentials above → **200**, returns JWT; wrong password → **401**.
- `GET api/Test` (JWT) with token → **200**; without token → **401**.
- `GET api/LibraryBook/Paged?Page=1&PageSize=5&listLostAndStolen=false` → **200**, real rows from MariaDB (Dapper path).
- `dotnet build` → **0 warnings, 0 errors** in both Debug and Release.
- **NuGet vulnerability scan** (`dotnet list package --include-transitive --vulnerable`) → **no vulnerable packages** in any of the 3 projects.

> ⚠️ **Known pre-existing issue (NOT caused by the upgrade):** `GET api/User/UserInfo` throws a 500 because
> `UserRepository.GetUserByUserName` (`UserRepository.cs:123`) joins legacy **`Company`/`Branch`** tables that
> do not exist in `LibraryDb` (leftover from the older Orion-suite schema). It would have failed identically
> before the migration. Fixing it requires either creating those tables or removing the join — decided to be
> out of scope for the .NET 10 upgrade.

> **Note:** to run again later, start MariaDB (`brew services start mariadb`) then
> `dotnet run --project LibraryAPIApp/LibraryAPIApp.csproj` (listens on `https://localhost:5001`).

---

## 1. Executive Summary

**Yes, the app can be fully upgraded to .NET 10.** The codebase is a small, self-contained solution
(3 projects, no external deployment infrastructure), so a full upgrade is low-risk.

| Project | Current TFM | Target TFM |
|---|---|---|
| `LibraryAPIApp` (Web API) | `netcoreapp3.0` | `net10.0` |
| `Common` (shared library) | `netstandard2.1` | `net10.0` |
| `DataAccess` (data layer) | `netstandard2.1` | `net10.0` |

Per the request, `netstandard2.1` is **dropped** — Common and DataAccess move directly to `net10.0`.
This is acceptable because all three projects only consume one another (no external `netstandard`
consumers exist in or outside this repo).

**Critical constraint — MySQL EF Core provider:**
`Pomelo.EntityFrameworkCore.MySql` (currently 3.1.0) has **no release for EF Core 10** (latest is
`9.0.0`, which targets EF Core 9). Two viable options exist (see §4). The plan recommends switching
to the official Oracle provider **`MySql.EntityFrameworkCore` 10.0.9**, which supports EF Core 10.
Fallback: keep Pomelo 9.0.0 with EF Core 9 packages (still runs on the .NET 10 runtime).

---

## 2. Current State

### 2.1 Projects and dependencies

```
LibraryAPIApp (netcoreapp3.0)
├── Common        (netstandard2.1)  ── Newtonsoft.Json 12.0.3, System.Configuration.ConfigurationManager 4.7.0,
│                                      System.ComponentModel.Annotations 4.7.0
└── DataAccess    (netstandard2.1)  ── Common
                                       Dapper 2.0.30, MySqlConnector 0.61.0,
                                       Microsoft.Extensions.Identity.Stores 3.1.0, Microsoft.Data.SqlClient 1.1.0
```

LibraryAPIApp packages (all end-of-life versions):
`MediatR 7.0.0`, `MediatR.Extensions.Microsoft.DependencyInjection 7.0.0`,
`Microsoft.AspNetCore.Identity.EntityFrameworkCore 3.1.0`, `Microsoft.EntityFrameworkCore 3.1.0`,
`Microsoft.EntityFrameworkCore.Tools 3.1.0`, `Pomelo.EntityFrameworkCore.MySql 3.1.0`,
`Microsoft.AspNetCore.Authentication.JwtBearer 3.0.0`, `System.IdentityModel.Tokens.Jwt 5.6.0`,
`Newtonsoft.Json 12.0.3`.

### 2.2 Known issues found during planning (will be fixed)

1. **Middleware bug:** `UseAuthorization()` is called **before** `UseAuthentication()`
   (`Startup.cs:119-120`) — reverse order breaks authorization for unauthenticated requests.
2. **ServiceLocator anti-pattern:** `ServiceLocator` builds a `ServiceProvider` inside
   `ConfigureServices` (`Startup.cs:75-76`); `IdentityDb` resolves `IConfiguration` through it
   (`ApplicationDbContext.cs:42`).
3. **Hardcoded secrets:** JWT key `"ZackTechSecretKey"` and issuer/audience are hardcoded
   (`ServicesConfiguration.cs:70-72`, `TokenController.cs:75-91`); default admin password
   `"Password123@"` is hardcoded in `DbInitializer.cs:101`.
4. **Unused/dead code:** `Newtonsoft.Json` referenced in 2 csproj files but used nowhere;
   `AppConfig .cs` + `IAppConfig.cs` are dead code (only referenced in comments) yet pull in
   `System.Configuration.ConfigurationManager`; `System.ComponentModel.Annotations` not needed on
   `net10.0` (part of the BCL).
5. **Ambiguous/duplicate imports:** `RepositoryBase.cs:10` imports `System.Data.SqlClient` alongside
   `Microsoft.Data.SqlClient` (line 16) — `new SqlConnection(...)` is ambiguous (CS0104).
6. **Duplicate DbContext registration:** `services.AddTransient<IdentityDb>()`
   (`ServicesConfiguration.cs:33`) duplicates `AddDbContext<IdentityDb>()`.
7. **`IdentityDb` anti-patterns:** parameterless ctor + `OnConfiguring()` + `UseMySql(...)`
   with no provider version, no connection string config via DI.
8. **DbContext shadow dependency:** `ApplicationUser`/`IdentityDb` reference `Microsoft.Extensions.Identity.Stores`
   transitively via packages that will be removed.
9. **launchSettings** launches `weatherforecast` route (removed in .NET 10 template).
10. **Migrations:** snapshot files are EF Core 3.1 format. Unused at runtime (`EnsureCreated` is used),
    but will not compile under EF Core 10 (see §4.3).

---

## 3. Preconditions

- [ ] .NET 10 SDK installed — **verified**: `10.0.400` is present at `/usr/local/share/dotnet/sdk`.
- [ ] A backup branch/tag of the repo (e.g. `git tag backup/pre-net10`).
- [ ] Confirm the DB connection string (currently only in User Secrets / UserSecrets ID
      `a51bb98d-11ba-4000-977f-5e7d0472fdad`) so the app can run after migration.
- [ ] Decide MySQL provider option (§4) — **recommended: Option A** (Oracle `MySql.EntityFrameworkCore`).

---

## 4. Decision Points

### 4.1 MySQL EF Core provider (BLOCKING)

EF Core is used **only** for ASP.NET Identity (user/roles/claims). Dapper handles all business tables.

| Option | Package | Version | Notes |
|---|---|---|---|
| **A (recommended)** | `MySql.EntityFrameworkCore` (Oracle) | `10.0.9` | Matches EF Core 10. Low risk because EF is used only for Identity. May have minor API-name differences for `UseMySql` (see §6.1.6). |
| **B (fallback)** | `Pomelo.EntityFrameworkCore.MySql` | `9.0.0` | Keep Pomelo, but EF Core packages must stay at 9.0.x. Runs on .NET 10 runtime but with EF Core 9. Pomelo 10.x not yet released (verified on NuGet, only 9.0.0 is latest). |

Choose **A** to get a fully up-to-date EF Core 10. If `MySql.EntityFrameworkCore` proves to have
runtime issues with your existing `AspNetUsers`/`AspNetRoles` schema, switch to **B** (only package
versions change; the `UseMySql` call site is the same).

### 4.2 JWT / Identity shared-framework packages

- `Microsoft.AspNetCore.Authentication.JwtBearer` is **no longer part of the shared framework**
  since .NET 8 — it must remain an explicit `PackageReference` at version `10.0.x`.
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` likewise becomes an explicit `10.0.x` reference
  (keep it explicit for clarity and to avoid ambiguity).

### 4.3 EF migrations vs `EnsureCreated`

Current behavior: `EnsureCreated()` on startup (no migrations). The `Migrations/` folder is EF Core
3.1-format and unused at runtime.

- **Recommended (keep current behavior):** keep `EnsureCreated`, **delete** the stale `Migrations/`
  folder and the `DbContextExtension.cs` file (dead code, uses old EF internal APIs). This is the
  lowest-risk path and preserves the existing schema.
- **Future follow-up (best practice):** migrate to formal EF migrations. Broken `EnsureCreated` into
  an initial migration (`dotnet ef migrations add InitialCreate`), then `Database.Migrate()`. Do this
  as a separate task after the upgrade is verified — a non-trivial schema/behavior change.

### 4.4 Hosting model

- **Option 1 (recommended):** modernize to the minimal hosting model
  (`WebApplication.CreateBuilder`) and inline `ConfigureServices`/`Configure` as `builder.Services` /
  `app.` calls. Consolidates `Program.cs` + `Startup.cs`. This is the .NET 10 best practice.
- **Option 2 (minimal change):** keep `CreateHostBuilder` + `Startup.cs`. Still fully supported, fewer
  diffs.

The plan below uses **Option 1** but is written so Option 2 deviates only in §6.2.

---

## 5. Target Package Matrix

### 5.1 `LibraryAPIApp.csproj` (net10.0)

| Package | Old | New | Action |
|---|---|---|---|
| MediatR | 7.0.0 | 14.2.0 | Update |
| MediatR.Extensions.Microsoft.DependencyInjection | 7.0.0 | — | **Remove** (DI built into MediatR since v12) |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 3.1.0 | 10.0.x | Update |
| Microsoft.EntityFrameworkCore | 3.1.0 | 10.0.x | Update |
| Microsoft.EntityFrameworkCore.Tools | 3.1.0 | 10.0.x | Update (keep for future migrations) |
| Pomelo.EntityFrameworkCore.MySql | 3.1.0 | — | **Remove** (or stay 9.0.0 if Option B) |
| MySql.EntityFrameworkCore | — | 10.0.9 | **Add** (Option A) |
| Microsoft.AspNetCore.Authentication.JwtBearer | 3.0.0 | 10.0.x | Update (explicit ref required) |
| System.IdentityModel.Tokens.Jwt | 5.6.0 | 8.22.0 | Update |
| Newtonsoft.Json | 12.0.3 | — | **Remove** (unused, has known high-severity advisory GHSA-5crp-9r3c-p9vr) |

### 5.2 `Common.csproj` (net10.0)

| Package | Old | New | Action |
|---|---|---|---|
| Newtonsoft.Json | 12.0.3 | — | **Remove** (unused) |
| System.Configuration.ConfigurationManager | 4.7.0 | — | **Remove** (only used by dead `AppConfig .cs`, see §6.1.3) |
| System.ComponentModel.Annotations | 4.7.0 | — | **Remove** (part of .NET 10 BCL) |

### 5.3 `DataAccess.csproj` (net10.0)

| Package | Old | New | Action |
|---|---|---|---|
| Dapper | 2.0.30 | 2.1.86 | Update |
| MySqlConnector | 0.61.0 | 2.x (latest) | Update |
| Microsoft.Extensions.Identity.Stores | 3.1.0 | 10.0.x | Update |
| Microsoft.Data.SqlClient | 1.1.0 | 7.0.3 | Update |

> **Note on chosen package versions:** versions were checked against NuGet on 2026-09-13.
> Re-confirm with `dotnet list package` / `dotnet package search` before applying.

---

## 6. Step-by-Step Implementation

### Phase 1 — Tooling

1. Add `global.json` at the repo root to pin the SDK:
   ```json
   {
     "sdk": {
       "version": "10.0.400",
       "rollForward": "latestFeature"
     }
   }
   ```
2. Barrier: `dotnet --version` → `10.0.400`.

### Phase 2 — Project files

3. `LibraryAPIApp.csproj`: `<TargetFramework>net10.0</TargetFramework>`. Remove
   `Microsoft.Extensions.Identity.EntityFrameworkCore`? **No** — keep but update to 10.0.x.
   Apply the matrix in §5.1. Remove the empty `<ItemGroup>` blocks and the stale
   `<Compile Remove="Data\Schema\Tables.cs" />` if `Data/Schema/Tables.cs` no longer exists
   (verify before deleting).
4. `Common.csproj`: `<TargetFramework>net10.0</TargetFramework>`; remove all three packages (§5.2).
5. `DataAccess.csproj`: `<TargetFramework>net10.0</TargetFramework>`; apply §5.3.
6. Optional (best practice): add a root `Directory.Build.props`:
   ```xml
   <Project>
     <PropertyGroup>
       <LangVersion>latest</LangVersion>
       <ImplicitUsings>enable</ImplicitUsings>
       <Nullable>enable</Nullable>
       <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
     </PropertyGroup>
   </Project>
   ```
   > `Nullable=enable` and `ImplicitUsings=enable` are best practice but will surface many warnings
   > in legacy code. Safer initial step: enable `ImplicitUsings` only; enable `Nullable` as a separate,
   > dedicated clean-up task later (§8). If added now, expect to fix warnings incrementally.
7. Restore and build to reveal remaining compile errors:
   ```bash
   dotnet restore LibraryAPIApp.sln
   dotnet build LibraryAPIApp.sln
   ```

### Phase 3 — Compile fixes

Work through errors in dependency order (Common → DataAccess → LibraryAPIApp).

8. **`RepositoryBase.cs`** — resolve CS0104 ambiguity:
   - Remove `using System.Data.SqlClient;` (line 10), keep `using Microsoft.Data.SqlClient;`.
9. **`AppConfig .cs`** (Common) — delete file. Update `IAppConfig.cs`:
   - If `IAppConfig` is not used anywhere else (only referenced in a commented-out line in
     `ServicesConfiguration.cs:28`), delete `IAppConfig.cs` too and remove the `using Common.Configuration;`
     comment.
   - This removes the last consumer of `System.Configuration.ConfigurationManager`.
10. **Migrations cleanup** (if keeping `EnsureCreated`, §4.3):
    - Delete the `Migrations/` folder (3.1-format, uncompilable under EF Core 10).
    - Delete `Data/DbContextExtension.cs` (references `IHistoryRepository`/`IMigrationsAssembly`,
      internal in modern EF — dead code).
11. **`DbContext` (`ApplicationDbContext.cs`/`IdentityDb`)** — see §6.1.6 refactor.
12. **MediatR** — `ServicesConfiguration.cs:24`:
    ```csharp
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    ```
    Remove the `MediatR.Extensions.Microsoft.DependencyInjection` package (§5.1).

### Phase 4 — Best-practice fixes (behavioral)

13. **Middleware order** (`Startup.cs`, or new Program): Authentication before Authorization:
    ```csharp
    app.UseAuthentication();
    app.UseAuthorization();
    ```
14. **Remove ServiceLocator** (`Util/ServiceLocator.cs`, `Startup.cs:75-76`) — see DbContext refactor
    (§6.1.6). Delete the file once nothing references it; remove the `#pragma warning disable ASP0000`.
15. **`ApiConfiguration`** — confirm `DefaultTimeout`/`RDBMS` shaping via config; keep as-is but bind from
    `appsettings.json` + env vars (currently appsettings.json is empty `{}` — seeding may have relied
    on User Secrets).
16. **Template code** — remove `WeatherForecast.cs` and `WeatherForecastController.cs` (template leftover).
    Update `launchSettings.json` `launchUrl` from `weatherforecast` to a real route (e.g. `api/Test`
    or `api/LibraryBook`), or remove `launchUrl`.

### Phase 5 — Configuration & secrets (security hardening)

17. **JWT secret** — move `"ZackTechSecretKey"`, issuer, audience out of code into
    configuration (User Secrets / env vars / appsettings). `ServicesConfiguration.cs` should do:
    ```csharp
    var jwt = Configuration.GetSection("Jwt");
    // jwt["Issuer"], jwt["Audience"], jwt["SecretKey"]
    ```
    `TokenController.cs:75-91` must read the same config instead of literals.
18. **Default admin password** — move `"Password123@"` (`DbInitializer.cs:101`) to a configuration
    value (e.g. `DefaultAdmins:Password`), fail fast if missing in Production.
19. **Default admin emails** — already read from `DefaultAdmins:DbAdmins`; keep, but document that
    `appsettings.json` must define them (currently empty).
20. **`ApiConfiguration`/connection strings** — the app is currently unusable from bare config
    (`appsettings.json` is `{}`); document the required config keys. Do **not** commit real secrets —
    use User Secrets / environment variables.

### Phase 6 — DbContext refactor (removes ServiceLocator)

21. Rewrite `IdentityDb` (`ApplicationDbContext.cs`):
    - Delete the parameterless ctor, the `Create()` factory, and the `OnConfiguring()` override.
    - Constructor takes only `DbContextOptions<IdentityDb>`.
    - Delete `GetConnectionString()` (remove `IConfiguration` resolution via ServiceLocator).
    - Keep `OnModelCreating` as-is.
22. In services registration (`Startup`/Program):
    ```csharp
    var cnn = Configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<IdentityDb>(o =>
    {
        if (rdbms == "MySQL") o.UseMySql(cnn, ServerVersion.AutoDetect(cnn)); // Oracle provider: o.UseMySQL(cnn)
        else o.UseSqlServer(cnn);
    });
    ```
    - Match the provider `UseMySql`/`UseMySQL` signature to the chosen package (§4.1).
    - With Oracle `MySql.EntityFrameworkCore`, the extension is `UseMySQL` (note casing).
23. Remove `services.AddTransient<IdentityDb>()` from `ServicesConfiguration.cs:33`
    (double registration; leave registration to `AddDbContext`).
24. Delete `Util/ServiceLocator.cs` once no references remain. Verify with
    `dotnet build` and grep for `ServiceLocator`.

### Phase 7 — Hosting model (Option 1: minimal hosting, recommended)

25. Rewrite `Program.cs`:
    ```csharp
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddDbContext<IdentityDb>(...);          // from §6.1.6
    builder.Services.AddCustomServices(...);                // adapt signature to accept config
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<IdentityDb>()
        .AddUserManager<UserManager<ApplicationUser>>();
    builder.Services.Configure<IdentityOptions>(...);
    builder.Services.AddJwtBearerServices(builder.Configuration); // adapt to read config
    builder.Services.AddAuthorization(...policies...);

    var app = builder.Build();

    if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
    app.UseHttpsRedirection();
    app.UseAuthentication();   // order matters
    app.UseAuthorization();
    app.MapControllers();

    EnsureDatabase(app);       // moved from Startup.Configure
    app.Run();
    ```
26. Delete `Startup.cs`. Move the two private methods (`EnsureDatabase`) into `Program.cs`
    (as `async Task`; replace `.Wait()` with `await`).
27. In `EnsureDatabase`, replace the sync `.Wait()` with `await`.

### Phase 8 — Build, restore, verify

28. ```bash
    dotnet restore LibraryAPIApp.sln
    dotnet build LibraryAPIApp.sln
    dotnet build LibraryAPIApp.sln -c Release
    ```
    Fix any residual warnings from newer analyzers (see §6). Aim for zero severity warnings.
29. Static verification of vulnerable references:
    ```bash
    dotnet list LibraryAPIApp/LibraryAPIApp.csproj package --include-transitive --vulnerable
    ```
    Expect no high-severity advisories. (Declared goal: none for `Newtonsoft.Json`, which is removed.)
30. Run the app: `dotnet run --project LibraryAPIApp` and exercise:
    - `POST api/Token` (login) returns a JWT.
    - `GET api/LibraryBook` with bearer token returns 200; without token returns 401.
    - `GET api/User` returns user info after login.
    - DB seeded (Users/AspNetRoles populated) on first start.

---

## 7. Verification Checklist

- [ ] `dotnet --version` reports `10.0.400` and `global.json` pins it.
- [ ] All 3 csproj target `net10.0`.
- [ ] No package references remain below the .NET 10-era minimums (§5 matrix).
- [ ] `RepositoryBase.cs` recompiles (SqlClient ambiguity removed).
- [ ] ServiceLocator gone: `grep -ri servicelocator . --include=*.cs` → no hits.
- [ ] Middleware order is Authentication → Authorization.
- [ ] JWT key/issuer/audience and default-admin password come from configuration, not literals.
- [ ] `Newtonsoft.Json` absent from `obj/project.assets.json` after restore.
- [ ] `dotnet list package --vulnerable` reports no high/medium severity advisories.
- [ ] Login + authenticated CRUD endpoints verified manually against the existing DB.
- [ ] `WeatherForecast*` removed; launchSettings `launchUrl` updated.

---

## 8. Risks & Rollback

| Risk | Mitigation |
|---|---|
| MySQL provider differences (Oracle vs Pomelo) | EF used only for Identity; switch Option A→B in `UseMySql` call and 2 csproj lines (minor). |
| EF Core 10 model differences | Identity schema is standard; `EnsureCreated` rebuilds/uses existing schema unchanged. Snapshot deleted, so no migration mismatch. |
| `Nullable=enable` flood of warnings | Stage it separately (§6 Step 6 note); keep initial upgrade `Nullable` off. |
| Existing database with old schema | `EnsureCreated` only creates-if-missing; existing table row counts preserved (`context.Users.Any()` gates seeding). |
| `MySql.EntityFrameworkCore` `UseMySQL` API name | Check extension method after restore; if absent, `UseMySql` still available via Pomelo fallback. |
| SDK roll-forward surprises | `global.json` pins; `rollForward: latestFeature`. |

**Rollback:** keep a pre-upgrade branch/tag (`git tag backup/pre-net10`, Phase 0). The migration is
confined to `.csproj`, `Program.cs`/`Startup.cs`, `ServicesConfiguration.cs`, `ApplicationDbContext.cs`,
`DbInitializer.cs`, `TokenController.cs`, and a few util files — simple `git revert`/checkout restores.

---

## 9. Recommended Follow-ups (after upgrade verified)

1. Enable `Nullable` reference types project-wide + resolve warnings.
2. Adopt **EF Core migrations** (replace `EnsureCreated`) for schema management.
3. Add `.editorconfig` for consistent style, and CI (GitHub Actions) running `dotnet build` +
   `dotnet list package --vulnerable`.
4. Add **structured logging** (`ILogger` instead of `Console.WriteLine` in `ServicesConfiguration.cs:79-86`).
5. Add **API versioning** (`Asp.Versioning.Mvc`) and an OpenAPI spec (`Microsoft.AspNetCore.OpenApi`),
   which is now the .NET 10 template standard.
6. Add unit tests for `JwtTokenBuilder`, `RandomKeyGenerator`, `RijndaelCrypt`, and the managers.
7. Consider **Central Package Management** (`Directory.Packages.props`) if more projects are added.
8. Convert `LibraryAPIApp.sln` to `.slnx` (new default) — optional housekeeping.