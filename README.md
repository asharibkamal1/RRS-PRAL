# PRAL PER 2026 — Performance Evaluation Report System

Enterprise web application for PRAL/FBR that digitizes the annual employee performance
evaluation cycle (Goals 70% + 360° Peer Competency 30%). Built with **.NET 10**, **Blazor**,
**MudBlazor**, **EF Core 10** and **SQL Server**, on a modular **Clean Architecture** base
with **Repository + Unit of Work**, a **service layer**, and a **stored-procedure** data path
for the Database team's SPs.

> Status: **all three role modules implemented** — Employee, Manager and Admin screens
> (dashboards, profiles, goals, 360° rating, attribute/level administration, period setup,
> assign raters, PER reports, system flow). See `docs/` for the spec, plans and the
> code-review/cleanup notes.

## Solution layout (Clean Architecture)

```
PralPer.sln
├── src/
│   ├── PralPer.Domain          # Entities, enums, constants (no deps). NO scoring — calc is DB-side.
│   ├── PralPer.Application      # Abstractions (IRepository, IUnitOfWork, ICurrentUser,
│   │                            #   IStoredProcedureExecutor), service interfaces, Result
│   ├── PralPer.Infrastructure   # EF Core DbContext + configs, Identity, repositories,
│   │                            #   Unit of Work, Dapper SP executor, seeding, services
│   └── PralPer.Web             # Blazor (MudBlazor) UI — modular Admin/Manager/Employee
│                                #   pages + layouts, auth controller, claims plumbing
└── tests/
    └── PralPer.UnitTests        # xUnit: domain scoring (PerScale) + service tests
                                 #   over an in-memory SQLite AppDbContext
```

> **Calculation policy:** the app performs **no PER score calculation**. All scores/percentages are
> computed in the **database** (stored procedures) and stored in tables; the app only **inserts raw
> inputs** and **fetches results** to display. See `docs/Architecture-Decisions.md` (AD-1).

**Dependency rule:** `Web → Infrastructure → Application → Domain`. Domain depends on nothing.
The DB-team schema swap is isolated to Infrastructure.

## Key building blocks
- **Repository + Unit of Work** — `IRepository<T>` / `IUnitOfWork` (Application) implemented in Infrastructure.
- **Stored procedures** — `IStoredProcedureExecutor` (Dapper) shares the EF connection for the DB team's SPs.
- **Identity** — ASP.NET Core Identity, app-managed accounts, **3 roles** (Admin/Manager/Employee),
  multi-role accounts with a **"Continue as"** screen and a **"Viewing as"** switch (active-role claim via cookie + `IClaimsTransformation`).
- **Service layer** — feature interfaces in Application, data-bound implementations in Infrastructure; Blazor pages depend on the interfaces only.
- **Modular UI** — separate `Components/Pages/{Admin,Manager,Employee}` modules, role-aware nav, role dashboards.

## Run on a new machine (step by step)

> **TL;DR:** install the .NET 10 SDK + a SQL Server, set the connection string, then
> `dotnet run --project src/PralPer.Web`. The database is **created, migrated and seeded
> automatically on first run** — you do **not** need to run any migration command. The
> migrations already live in `src/PralPer.Infrastructure/Migrations/`.

### 1. Prerequisites
| Tool | Notes |
|---|---|
| **.NET 10 SDK** | `dotnet --version` should print `10.x`. Get it from https://dotnet.microsoft.com/download or, on Ubuntu 24.04, `sudo apt-get install -y dotnet-sdk-10.0`. |
| **SQL Server** | Any edition. **Windows:** LocalDB (ships with Visual Studio / the "Data storage and processing" workload) — the default connection string already targets it. **macOS / Linux:** LocalDB is **not** available — use Docker (below) or a SQL Server instance. |
| **EF Core CLI** *(optional)* | Only needed if you want to run migration commands manually: `dotnet tool install --global dotnet-ef --version 10.0.0` |
| **Git** | to clone the repository. |

### 2. Get the code
```bash
git clone https://github.com/<your-account>/RRS-PRAL.git
cd RRS-PRAL
```
*(Pushing it the first time: `git init && git add . && git commit -m "init" && git branch -M main && git remote add origin https://github.com/<you>/RRS-PRAL.git && git push -u origin main`.)*

### 3. Start a SQL Server (only if you don't already have one)
- **Windows (LocalDB):** nothing to do — the default works.
- **macOS / Linux (Docker):**
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
    -p 1433:1433 --name pralsql -d mcr.microsoft.com/mssql/server:2022-latest
  ```

### 4. Point the app at your database
The connection string lives in **`src/PralPer.Web/appsettings.json`** → `ConnectionStrings:DefaultConnection`.

- **Windows / LocalDB (default — leave as-is):**
  ```
  Server=(localdb)\MSSQLLocalDB;Database=PralPerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
  ```
- **Docker / SQL Server with SA login** — change it to:
  ```
  Server=localhost,1433;Database=PralPerDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=true
  ```
> Tip: keep secrets out of source control with user-secrets instead of editing the file:
> `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-conn-string>" --project src/PralPer.Web`

### 5. Restore, then run
```bash
dotnet restore                          # downloads all NuGet packages
dotnet run --project src/PralPer.Web
```
On first run the app will: **apply all migrations → create `PralPerDb` → seed reference + demo data**.
Open the `https://localhost:49330` URL it prints and sign in with a demo account below.

### 6. (Optional) Run the tests
```bash
dotnet test          # 42 tests; uses an in-memory SQLite db, no SQL Server needed
```

## Demo accounts (seeded)

| Email | Password | Roles |
|---|---|---|
| `employee@pral.com.pk` | `Pral@12345` | Employee |
| `manager@pral.com.pk`  | `Pral@12345` | Manager + Employee |
| `admin@pral.com.pk`    | `Pral@12345` | Admin + Employee |

Multi-role accounts land on **Continue as** to pick the active role; single-role accounts go
straight to their dashboard. Use **Switch** (top bar) to change the active role.

## Working with migrations (only when you change entities)

You do **not** run these for a normal clone/run — migrations apply automatically at startup.
Use them only after editing an entity or `AppDbContext`:

```bash
# add a migration capturing your entity changes
dotnet ef migrations add <DescriptiveName> -p src/PralPer.Infrastructure -s src/PralPer.Web

# (optional) apply migrations to the DB without launching the app
dotnet ef database update -p src/PralPer.Infrastructure -s src/PralPer.Web

# see the list / whether the model is in sync with the snapshot
dotnet ef migrations list -p src/PralPer.Infrastructure -s src/PralPer.Web
dotnet ef migrations has-pending-model-changes -p src/PralPer.Infrastructure -s src/PralPer.Web
```

## Troubleshooting

- **`LocalDB is not supported on this platform`** — you're on macOS/Linux. LocalDB is Windows-only;
  start SQL Server via Docker (step 3) and update the connection string (step 4).
- **`Login failed` / `A network-related error`** — SQL Server isn't reachable or the credentials are
  wrong. Verify the container is running (`docker ps`) and the `Server`/`User Id`/`Password` match.
- **`PendingModelChangesWarning: ... has pending changes`** — the model drifted from the snapshot
  after an entity change. Add a migration to capture it:
  `dotnet ef migrations add <Name> -p src/PralPer.Infrastructure -s src/PralPer.Web`.
  (The repo's model is currently **in sync**; `AddDbContext` also ignores this warning so startup
  never hard-fails on it.)
- **Start completely fresh** — drop and let it re-seed:
  `dotnet ef database drop -f -p src/PralPer.Infrastructure -s src/PralPer.Web` then `dotnet run`.
- **Port already in use** — pass another: `dotnet run --project src/PralPer.Web --urls http://localhost:5080`.

## Notes
- Phase 0 screens render via **static server-side rendering**; interactivity (live weight-sum on
  Goal Submission, 360° sliders, dialogs) is enabled per-page (`@rendermode InteractiveServer`)
  as those Employee screens are built.
- When the Database team provides production tables/SPs, map them in **Infrastructure** only;
  Domain/Application/Web are unaffected.
