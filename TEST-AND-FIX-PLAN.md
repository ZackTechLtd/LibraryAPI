# Test & Fix Plan: UserInfo 500 + xUnit Test Project

> **Date:** 2026-09-13 (rev. 2)
> **Base:** Post-.NET-10 upgrade (see `UPGRADE-PLAN.md` — completed & verified).
> **Target SDK:** .NET 10 (pinned `10.0.400` via `global.json`)
> **Status:** 📋 PLANNED — no code changes made yet.

---

## 0. Scope

Two deliverables:

1. **Fix the known pre-existing 500** on `GET api/User/UserInfo`.
   Root cause: `UserRepository` queries reference the legacy `Company`/`Branch` tables **and**
   `AspNetUsers.CompanyId`/`AspNetUsers.BranchId` columns, none of which exist in the `LibraryDb`
   schema. Fix approach agreed with owner (this revision): **create the legacy tables + columns and
   seed them appropriately** — rather than removing the joins — so `UserInfo` and the other
   legacy-table methods (`GetUsersPaged`, `UpdateUser`, `DeleteUser`,
   `GetNumberOfCompanyUsersByBranchCode`, …) all work.
2. **Add a new xUnit test project** (`LibraryAPIApp.Tests`) to the solution containing:
   - **Unit tests** (pure logic: `JwtTokenBuilder`, `JwtSecurityKey`, `RandomKeyGenerator`,
     `RijndaelCrypt`, managers mocked with Moq).
   - **Functional tests** (real HTTP against the app via `WebApplicationFactory`, backed by a
     **Testcontainers MariaDB** container, seeding a **config-driven** test user + company/branch).

Tooling decisions (confirmed with owner):
| Decision | Choice |
|---|---|
| UserInfo fix strategy | **Add `Company`/`Branch` tables + `AspNetUsers` columns; seed them** (keep joins) |
| xUnit version | **xUnit 3.x** (SDK 10 template, Microsoft Testing Platform) |
| Mocking library | **Moq** |
| Functional-test DB | **Testcontainers + Docker MariaDB** (isolated, CI-friendly) |
| Test credentials / seed data | **Config-driven** (no hardcoded secrets in test code) |

> ⚠️ This plan makes **code changes** — it is delivered as an md file only; none of the steps below are
> applied yet. No production code is altered by the plan author; all edits happen when implementing it.

---

## 1. Part 1 — Fix `GET api/User/UserInfo` (500)

### 1.1 Root cause

`UserController.GetUserInfo` (`LibraryAPIApp/Controllers/UserController.cs:28`) calls:

1. `UserWebApiManager.GetUserByUserName(username)` → `UserRepository.GetUserByUserName`
   (`DataAccess/WebApiRepository/Repository/UserRepository.cs:113`). SQL (line 123):
   `LEFT OUTER JOIN Company C ON U.CompanyId = C.CompanyId` +
   `LEFT OUTER JOIN Branch B ON U.BranchId = B.BranchId`. Both fail:
   - `Table 'LibraryDb.Company' doesn't exist` → unhandled → **500**.
2. `UserWebApiManager.GetUserAndRolesByUserName(username, ...)` → `UserRepository.GetUserAndRolesByUserName`
   (line 326). SQL (line 363) also joins `Branch B` → same failure class.

Important: the queries also reference `U.CompanyId` and `U.BranchId` **on `AspNetUsers`**. Evidence from
history (`git show 152cfc1` — EF 3.1 `InitialCreate` migration) confirms `AspNetUsers` was created with
**only** the standard Identity columns plus `LastPasswordChangedDate` — **no** `CompanyId`/`BranchId`.
`Company`/`Branch`/`LastUserCompanyAndBranch` are not defined anywhere in this repo (the only business
schema in history is `Data/Schema/Tables.txt` for `LibraryBook`/`LibraryUser`/`LibraryBookStatus`).
They come from the legacy **OrionWebSuite** DB.

So a complete fix requires BOTH:
- the legacy **tables** (`Company`, `Branch`, and `LastUserCompanyAndBranch` — the last for the
  `UpdateUser` path), **and**
- the **`AspNetUsers` columns** `CompanyId` / `BranchId`.

> ⚠️ **Pre-existing, NOT caused by the .NET 10 upgrade** (documented in `UPGRADE-PLAN.md` §0).
> Today the only controller calling into the user manager is `UserController`; `GetUsersPaged`,
> `UpdateUser`, `DeleteUser`, `GetNumberOfCompanyUsersByBranchCode`, etc. are exposed through
> `IUserWebApiManager`/`IUserRepository` but are **not reachable from any HTTP route** yet. Adding the
> tables makes the whole repository surface consistent and future-proof.

### 1.2 Target schema (what "add the tables" means)

| Object | Columns (from SQL usage) | Notes |
|---|---|---|
| `Company` | `CompanyId` INT PK identity, `CompanyCode` VARCHAR(40) UNIQUE, `CompanyName` VARCHAR(255) | Joined via `U.CompanyId` |
| `Branch` | `BranchId` INT PK identity, `CompanyId` INT FK → `Company`, `BranchCode` VARCHAR(40) UNIQUE, `BranchName` VARCHAR(255) | Joined via `U.BranchId` / `B.CompanyId` |
| `LastUserCompanyAndBranch` | `LastUserCompanyAndBranchId` INT PK identity, `UserName`, `CompanyId`, `BranchId`, `DateCreated`, `CreatedBy`, `DateModified`, `ModifiedBy` | Written by `UpdateUser` → `AddOrUpdateLastUserCompanyAndBranch` (line 1001) |
| `AspNetUsers` | **add** `CompanyId` INT NULL, `BranchId` INT NULL | Referenced as `U.CompanyId` / `U.BranchId` throughout |

`PreviousPassword` already exists as an EF entity (`DataAccess/IdentityModels/PreviousPassword.cs`) and
is created by `EnsureCreated`; the `DeleteUser` path only fails on MySQL due to the hardcoded
`[Identity].` prefix (see Task 3).

### 1.3 Implementation approach — Decision Point

| Option | Description | Pros / Cons |
|---|---|---|
| **A (recommended) — EF entities + idempotent upgrade** | Model `Company`, `Branch`, `LastUserCompanyAndBranch` as entities on `IdentityDb`; add `CompanyId`/`BranchId` scalars to `ApplicationUser`. Fresh databases (incl. functional-test containers) get the full schema from `EnsureCreated`. For the **existing dev DB**, run an idempotent upgrade SQL at startup (below) since `EnsureCreated` never mutates an existing DB. | Pros: schema single-source-of-truth in the EF model; tests get schema for free. Cons: must keep the bootstrap SQL in sync with the model. |
| **B — raw SQL only** | Keep `IdentityDb` untouched; add `Data/Schema/CompanyBranch.sql` (mirrors the existing `Tables.txt` pattern) executed at startup. | Pros: consistent with how business tables are created today; zero EF changes. Cons: schema knowledge lives in SQL; two sources of truth. |

Both options share the same idempotent upgrade SQL (used for existing DBs in Option A, always used in Option B):

```sql
CREATE TABLE IF NOT EXISTS Company (
  CompanyId   INT AUTO_INCREMENT PRIMARY KEY,
  CompanyCode VARCHAR(40)  NOT NULL,
  CompanyName VARCHAR(255) NOT NULL,
  UNIQUE KEY uq_Company_CompanyCode (CompanyCode)
);

CREATE TABLE IF NOT EXISTS Branch (
  BranchId   INT AUTO_INCREMENT PRIMARY KEY,
  CompanyId  INT NOT NULL,
  BranchCode VARCHAR(40)  NOT NULL,
  BranchName VARCHAR(255) NOT NULL,
  UNIQUE KEY uq_Branch_BranchCode (BranchCode),
  CONSTRAINT fk_Branch_Company FOREIGN KEY (CompanyId) REFERENCES Company (CompanyId)
);

CREATE TABLE IF NOT EXISTS LastUserCompanyAndBranch (
  LastUserCompanyAndBranchId INT AUTO_INCREMENT PRIMARY KEY,
  UserName                   VARCHAR(256) NOT NULL,
  CompanyId                  INT NOT NULL,
  BranchId                   INT NOT NULL,
  DateCreated                DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CreatedBy                  VARCHAR(255) NOT NULL,
  DateModified               DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ModifiedBy                 VARCHAR(255) NOT NULL
);

-- Only when the columns are absent (check information_schema.columns first):
ALTER TABLE AspNetUsers
  ADD COLUMN CompanyId INT NULL,
  ADD COLUMN BranchId  INT NULL;
```

### 1.4 Changes

**Task 1 — Schema** (per §1.3, **Option A recommended**):
- Add entities `Company`, `Branch`, `LastUserCompanyAndBranch` (recommended under
  `DataAccess/IdentityModels/`) with unique indexes on `CompanyCode` / `BranchCode` and the `Branch →
  Company` FK.
- Add nullable `int CompanyId` / `int BranchId` to `ApplicationUser`
  (`DataAccess/IdentityModels/ApplicationUser.cs`) — plain scalar columns, no navigations needed
  (matches how Dapper consumes them).
- Register `DbSet<Company>`, `DbSet<Branch>`, `DbSet<LastUserCompanyAndBranch>` on `IdentityDb`
  (enables `EnsureCreated` + EF seeding).
- Add the idempotent upgrade SQL above, executed in `Program.EnsureDatabase` after `EnsureCreated`
  (guarded by `information_schema` checks), so a pre-existing `LibraryDb` is upgraded in place.

**Task 2 — Seed** (`Data/DbInitializer.cs`, config-driven):
- Ensure one `Company` row exists (config `CompanySeed:CompanyCode/CompanyName`; defaults
  `TEST` / `Test Company`) — keyed upsert, skipped if present.
- Ensure one `Branch` row exists tied to that company (config `BranchSeed:BranchCode/BranchName`;
  defaults `MAIN` / `Main Branch`).
- When each seeded admin is created, set its `CompanyId`/`BranchId` from the seeded branch/company
  (so the `INNER JOIN Company` in `GetUsersPaged` and the seeded-count methods return rows).
- (Optional) record the assignment in `LastUserCompanyAndBranch`.
- Idempotent — mirrors the existing `context.Users.Any()` gate.

**Task 3 — MySQL-schema hardening for the legacy paths** (previously invisible because the tables were
missing; required now that the tables exist):
- `GetUsersCompanyAndBranchId` (`UserRepository.cs:1099`) hardcodes `[Identity].[AspNetUsers]`.
  Apply the same RDBMS table-name switch used elsewhere (`AspNetUsers` for MySQL).
- `DeleteUser` (`UserRepository.cs:939-956`) hardcodes `[Identity].PreviousPassword`,
  `[Identity].AspNetUserRoles/Claims/Logins`. Apply the same switch to all four.
- Leave every `Company`/`Branch` join intact — they now resolve to real tables.
- Out of scope (flagged, not touched): `UpdateUserOld` (`:750`) calls SQL Server SP
  `[Config].[UpdateUser]` — unreachable on MySQL.

### 1.5 Manual verification

1. Start MariaDB: `brew services start mariadb`.
2. `dotnet run --project LibraryAPIApp/LibraryAPIApp.csproj` (listens on `https://localhost:5001`).
3. Schema check: `SHOW TABLES LIKE 'Company%'`, `SHOW TABLES LIKE 'Branch%'`,
   `SHOW TABLES LIKE 'LastUserCompanyAndBranch'`, `DESCRIBE AspNetUsers` → `CompanyId`/`BranchId`
   present; seeded admin row has both set.
4. `POST https://localhost:5001/api/Token` with the seeded admin
   (`za1012001@yahoo.co.uk` / `Password123@`, from User Secrets) → 200 + JWT.
5. `GET https://localhost:5001/api/User/UserInfo?username=za1012001@yahoo.co.uk`
   with `Authorization: Bearer <token>` → **200**, JSON has `UserName`/`Email`, `IsAdmin: true`
   (previously 500).
6. `dotnet build LibraryAPIApp.sln` → 0 errors, 0 warnings.

---

## 2. Part 2 — Scaffold the xUnit test project

### 2.1 Preconditions

- [ ] Docker running (functional tests use `Testcontainers.MariaDb`).
- [ ] .NET 10 SDK as pinned (`dotnet --version` → `10.0.400`).
- [ ] `Program` is reachable by `WebApplicationFactory<TProgram>` — **requires one small change
       (see Task 6)**.

### 2.2 Project layout

```
LibraryAPIApp.Tests/                      (new project, root of repo)
├── LibraryAPIApp.Tests.csproj
├── Unit/                                 (pure-logic unit tests)
│   ├── JwtTokenBuilderTests.cs
│   ├── JwtSecurityKeyTests.cs
│   ├── RandomKeyGeneratorTests.cs
│   ├── RijndaelCryptTests.cs
│   ├── UserWebApiManagerTests.cs
│   └── LibraryBookWebApiManagerTests.cs
└── Functional/
    ├── CustomWebApplicationFactory.cs     (WebApplicationFactory<Program> + config overrides)
    ├── MariaDbTestContainer.cs            (ITestcontainers fixture, per-class or collection)
    ├── TokenFunctionalTests.cs
    ├── UserInfoFunctionalTests.cs
    ├── AnonymousAccessFunctionalTests.cs
    └── LegacyTablesFunctionalTests.cs     (verifies the seeded Company/Branch + legacy methods)
```

### 2.3 Create & add to solution

```bash
# Scaffold (SDK 10 ships the xUnit template; confirm template flavour with `dotnet new list`)
dotnet new xunit -o LibraryAPIApp.Tests --framework net10.0
# Adjust TFM + packages per §2.4 if the template emitted v2 packages.

# Add to existing solution
dotnet sln LibraryAPIApp.sln add LibraryAPIApp.Tests/LibraryAPIApp.Tests.csproj

# Add project references
dotnet add LibraryAPIApp.Tests reference LibraryAPIApp/LibraryAPIApp.csproj \
                                         Common/Common.csproj \
                                         DataAccess/DataAccess.csproj
```

### 2.4 Target package matrix (`LibraryAPIApp.Tests.csproj`, TFM `net10.0`)

| Package | Purpose | Versions |
|---|---|---|
| `xunit.v3` | xUnit 3.x test framework (Meta-package; brings Microsoft Testing Platform support) | latest stable 1.x |
| `xunit.runner.visualstudio` | VS / `dotnet test` integration | 3.x |
| `Microsoft.Testing.Extensions.CodeCoverage` | coverage (optional) | 17.x |
| `Moq` | Mocking for unit tests | 4.20.x |
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory<Program>` host for functional tests | 10.0.x |
| `Testcontainers` | Container orchestration base | 4.x |
| `Testcontainers.MariaDb` | MariaDB container module | 4.x |

Expected template PropertyGroups (keep/adjust from `dotnet new xunit` v3 output):
```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <OutputType>Exe</OutputType>                       <!-- xUnit v3 MTP runner -->
  <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
</PropertyGroup>
```

> **Versions were reasoned on 2026-09-13**; reconfirm with `dotnet package search` before implementing.

**Task 4 — Program(s) entry point change** (required for functional tests): append to the bottom of
`LibraryAPIApp/Program.cs`:
```csharp
public partial class Program { }
```
Top-level statements generate an *internal* `Program`; `WebApplicationFactory<Program>` needs it
public. No other change to `Program.cs`.

---

## 3. Part 3 — Unit tests (no external DB)

Pure in-memory tests against classes that need no I/O. Standard `[Fact]`s; Moq only for managers.

### 3.1 `JwtTokenBuilderTests` (LibraryAPIApp.Util)
- Building with valid `SecurityKey` (via `JwtSecurityKey.Create("ZackTechSecretKey")`), subject,
  issuer, audience → `Build()` returns `JwtToken` whose `Value` is a non-empty JWT; decode with
  `JwtSecurityTokenHandler` and assert `Sub`, issuer, audience.
- `AddClaim`/`AddClaims` round-trips into the token's claims.
- `AddExpiry(1)` → token expiry ≈ now + requested minutes (within tolerance).
- Missing security key / subject / issuer / audience → `ArgumentNullException`.

### 3.2 `JwtSecurityKeyTests` (LibraryAPIApp.Util)
- `Create("short-secret")` returns an `HmacSha256` key with `KeySize >= 256` bits (regression the
  upgrade fixed via SHA-256 derivation).
- Same input → same key bytes (deterministic); different inputs → different keys.

### 3.3 `RandomKeyGeneratorTests` (Common.Util)
- `GetUniqueKey(n)` → length `n`, chars only from the alphanumeric set.
- `CreateEmbededCustomerKey(secret)` → `GetEmbededCode(key)` recovers `secret` (position-prefix
  round-trip).
- `GetEmbededCode` on a malformed string → `null`.

### 3.4 `RijndaelCryptTests` (Common.Util)
- `Encrypt`→`Decrypt` round-trip for ASCII text using the default password.
- **Legacy-compat vector**: encrypt a known string with the default password and assert the exact
  base64 output (guards byte-compatibility with pre-upgrade encrypted data).
- `Decrypt("not-base64!")` → empty result + non-empty error tuple; disposed instance → error tuple.

### 3.5 Manager unit tests (Moq)
- `UserWebApiManager` construction with a mocked `IUserRepository`/`IConfiguration` +
  `IOptions<ApiConfiguration>`; each method (e.g. `GetUserByUserName`, `GetUsersPaged`,
  `GetUserAndRolesByUserName`) verifies delegation — mock returns fixed object, assert the manager
  returns it and the repo was called once.
- `LibraryBookWebApiManager` delegation tests (mock `ILibraryBookRepository`).

> Repository-level (Dapper SQL against real MariaDB) tests are **functional** (§4) — Dapper's
> `IDbConnection` extensions aren't mockable and unit tests wouldn't prove the fixed SQL.

---

## 4. Part 4 — Functional tests (Testcontainers + WebApplicationFactory)

### 4.1 Infrastructure pieces

**`MariaDbTestContainer`** (class fixture or collection fixture):
- `Testcontainers.MariaDb` container; map a free port; `Database=LibraryDb`, user/password set
  explicitly (container-config, not app secrets).
- `GetConnectionString()` → `Server=localhost;Port=<mapped>;Database=LibraryDb;Uid=...;Pwd=...;AllowLoadLocalInfile=true`
  (matches `MySqlConnector` expectations in `RepositoryBase.OpenConnection` and EF `UseMySQL`).
- `InitializeAsync` starts the container **before** the app host builds.

**`CustomWebApplicationFactory<Program>`** (`Microsoft.AspNetCore.Mvc.Testing`):
- `ConfigureWebHost` overrides config via `builder.UseSetting(...)` / in-memory `ConfigurationBuilder`:
  - `ConnectionStrings:DefaultConnection` → container connection string
  - `ApiConfiguration:RDBMS` → `MySQL`
  - `ApiConfiguration:DefaultTimeout` → `60`
  - `Jwt:Issuer` / `Jwt:Audience` / `Jwt:SecretKey` → test-only values (≥ 256-bit secret)
  - `DefaultAdmins:DbAdmins` → **`testadmin@library.test.test`** (config-driven test user)
  - `DefaultAdmins:Password` → test password meeting Identity policy (e.g. **`TestPassword123!`**)
  - `CompanySeed:*` / `BranchSeed:*` → the seeded company/branch (e.g. `TEST`/`MAIN`, cf. §1.2)
- On startup the app runs `EnsureDatabase` (`Program.cs:71`): `EnsureCreated` builds the schema
  (in Option A the new entities are included automatically), the idempotent upgrade SQL is a no-op on
  a fresh DB, and `DbInitializer` seeds roles, the admin, and the company/branch + assignment.

> All seed values, including the test user's credentials, come from **configuration overrides** —
> production User Secrets (real admin) are never read by tests.

### 4.2 HTTP functional test cases

| Test | Steps | Expected |
|---|---|---|
| Login succeeds | `POST /api/Token` `{username: testadmin@library.test.test, password: TestPassword123!}` | 200; body is a non-empty JWT |
| Login wrong password | same user, bad password | 401 |
| Login unknown user | unknown username | 401 |
| Anonymous rejected | `GET /api/Test` no Authorization header | 401 |
| Authenticated allowed | `GET /api/Test` with Bearer token | 200 |
| **UserInfo no longer 500** | `GET /api/User/UserInfo?username=testadmin@library.test.test` with Bearer | **200**; `isAdmin: true`, `userName`/`email` populated, non-empty `roleList`, `userClaims` present |
| (Optional) Dapper path | `GET /api/LibraryBook/Paged?Page=1&PageSize=5&listLostAndStolen=false` with Bearer | 200 |

### 4.3 Legacy-tables functional tests (seed verification, §4.1)

Run `UserRepository`/`UserWebApiManager` directly against the container DB (construct manually with
the same config overrides; no HTTP needed) and assert:

| Case | Assert |
|---|---|
| `GetUserByUserName(testadmin@…)` | non-null; `Email`/`UserName` populated — Company join no longer throws |
| `GetUserAndRolesByUserName(testadmin@…, testadmin@…)` | an `Administrator` role with `User != null` |
| `GetNumberOfCompanyUsersByBranchCode(MAIN)` | `1` (seeded branch has the admin assigned) |
| `GetUsersPaged(showAllUsers:true)` | admin row returned; `BranchCode == "MAIN"` |
| `GetUserOwnRolesByUserName` / `GetUserAndClaimsByUserName` | returns without error (Branch join OK) |
| `UpdateUser` (re-assign same branch; use a throwaway user) | returns `1`; `LastUserCompanyAndBranch` row upserted |
| `DeleteUser` (throwaway user) | returns `1` including `PreviousPassword` cleanup |

Share one container + one factory per test collection; recreate throwaway users per test.

---

## 5. Implementation order

| # | Task | Deliverable | Depends on |
|---|---|---|---|
| 1 | Add `Company`/`Branch`/`LastUserCompanyAndBranch` entities + `ApplicationUser.CompanyId/BranchId` (Option A) | schema model | — |
| 2 | Idempotent upgrade SQL in `Program.EnsureDatabase` (+ `Data/Schema/CompanyBranch.sql` if Option B) | existing-DB upgrade | 1 |
| 3 | Seed Company/Branch + assign admins in `DbInitializer` (config-driven) | seeded data | 1 |
| 4 | MySQL-schema hardening: `GetUsersCompanyAndBranchId`, `DeleteUser` RDBMS switch | legacy paths work on MariaDB | 3 |
| 5 | Append `public partial class Program {}` | `Program.cs` edit | — |
| 6 | `dotnet new xunit` + `dotnet sln add` + refs | `LibraryAPIApp.Tests.csproj` in solution | — |
| 7 | Unit tests (Utils + managers w/ Moq) | `Unit/*.cs` | 6 |
| 8 | Functional fixtures (container + factory) | `Functional/*.cs` | 5, 6 |
| 9 | HTTP functional tests | `Token/UserInfo/Anonymous/*Tests.cs` | 8 |
| 10 | Legacy-tables functional tests | `LegacyTablesFunctionalTests.cs` | 8, 4 |
| 11 | Build + run all tests | green `dotnet test` | 3–10 |

---

## 6. Build & verification checklist

- [ ] `dotnet build LibraryAPIApp.sln` → 0 errors, 0 warnings (Part 1 fix intact).
- [ ] `LibraryAPIApp.sln` contains `LibraryAPIApp.Tests` (project list + Debug/Release configs).
- [ ] Schema present in MariaDB: `Company`, `Branch`, `LastUserCompanyAndBranch`; `AspNetUsers` has
      `CompanyId`/`BranchId`; seeded admin assigned to the seeded company/branch.
- [ ] Manual check §1.5: `GET api/User/UserInfo` returns 200 (was 500).
- [ ] `dotnet test LibraryAPIApp.Tests` → all unit tests pass **without** Docker/DB running.
- [ ] `dotnet test LibraryAPIApp.Tests` with Docker up → all functional tests (HTTP + legacy tables)
      pass against the Testcontainers MariaDB.
- [ ] Re-running the app twice against the SAME `LibraryDb` is idempotent (upgrade SQL + seeding don't
      duplicate rows).
- [ ] Functional tests require no production User Secrets values; test user/seed is config-driven.
- [ ] `dotnet list package --include-transitive --vulnerable` on the test project → no advisories.
- [ ] No new warnings introduced by nullable-enabled test project referencing non-annotated production
      code (prefer none; suppressions only where necessary).

---

## 7. Risks & follow-ups

| Risk / note | Mitigation |
|---|---|
| `EnsureCreated` never mutates an existing DB → new tables/columns absent on the current `LibraryDb` | Idempotent upgrade SQL at startup (Task 2). Optionally drop & recreate `LibraryDb` once for tests. |
| EF model ↔ bootstrap-SQL drift over time | Functional tests create a fresh schema (EF path) on every run and exercise it — any drift fails tests. |
| Hardcoded `[Identity].` table prefixes break MariaDB | Task 4 makes affected methods use the RDBMS table-name switch; verify via §4.3. |
| Seeded company/branch collide with real rows in some deployment | Keyed upsert (`INSERT ... ON DUPLICATE KEY`) + skip-if-present; codes unique via unique keys. |
| `UpdateUserOld` (SQL Server SP `[Config].[UpdateUser]`) | Out of scope — unreachable on MySQL; flagged for removal or RDBMS port later. |
| Seeded/admin assignment changes behaviour of `GetNumberOfCompanyUsersByBranchCode` counts | Intentional — seed is the new SSOT; counts verified in §4.3. |
| xUnit v3 template/package differences on installed SDK | Confirm `dotnet new` template flavour and package versions at implementation time (§2.4 note). |
| Testcontainers needs Docker on the build machine | Unit tests always run without Docker; CI adds a Docker step for functional tests. |
| EF `UseMySQL` + `EnsureCreated` inside test host | Matches production startup path exactly (`Program.cs`); share one factory across the class if probe slow. |

**Rollback:** Part 1 is additive (new entities + seed + RDBMS-switch fixes in `UserRepository.cs`);
Part 2 is a new project + one appended line in `Program.cs`. Revertable via `git checkout -- <files>` /
removing the test project from the solution.

---

## 8. Recommended follow-ups (after this plan lands)

1. Adopt **EF Core migrations** to replace the `EnsureCreated` + bootstrap-SQL hybrid (schema becomes
   migration-managed end-to-end).
2. Port or delete the SQL Server-only leftovers (`UpdateUserOld`, any `[Config].`/`[Identity].` SP
   references) so the codebase is fully MariaDB-clean.
3. Wire `dotnet test` + `dotnet list package --vulnerable` into CI (GitHub Actions) with a Docker
   service for the functional suite.
4. Add code-coverage thresholds (`Microsoft.Testing.Extensions.CodeCoverage`) and a `.editorconfig`.
5. Consider Central Package Management (`Directory.Packages.props`) now that a 4th project exists.