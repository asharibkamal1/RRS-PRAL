# PRAL PER 2026 — Performance Evaluation Report System

Enterprise web application for PRAL/FBR that digitizes the annual employee performance
evaluation cycle (Goals 70% + 360° Peer Competency 30%). Built with **.NET 10**, **Blazor**,
**MudBlazor**, **EF Core 10** and **SQL Server**, on a modular **Clean Architecture** base
with **Repository + Unit of Work**, a **service layer**, and a **stored-procedure** data path
for the Database team's SPs.

> Status: **Phase 0 — foundation + Identity + 3 role dashboards**. Employee feature screens
> follow next, then Manager and Admin. See `docs/` for the full spec and plans.

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
    └── PralPer.UnitTests        # Domain/constant unit tests (xUnit)
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

## Prerequisites
- **.NET 10 SDK**
- **SQL Server** (LocalDB is fine — the default connection string targets `(localdb)\MSSQLLocalDB`)
- EF Core tools: `dotnet tool install --global dotnet-ef`

## First-time setup

1. **Restore & build**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Create the initial database migration** (entities → schema):
   ```bash
   dotnet ef migrations add InitialCreate -p src/PralPer.Infrastructure -s src/PralPer.Web
   ```

3. **Run** (migrations are applied and demo data seeded automatically at startup):
   ```bash
   dotnet run --project src/PralPer.Web
   ```
   Then open the printed `https://localhost:xxxx` URL.

   > The connection string lives in `src/PralPer.Web/appsettings.json`
   > (`ConnectionStrings:DefaultConnection`). Adjust it for your SQL Server instance if needed.

4. **Run tests**
   ```bash
   dotnet test
   ```

## Demo accounts (seeded)

| Email | Password | Roles |
|---|---|---|
| `employee@pral.com.pk` | `Pral@12345` | Employee |
| `manager@pral.com.pk`  | `Pral@12345` | Manager + Employee |
| `admin@pral.com.pk`    | `Pral@12345` | Admin + Employee |

Multi-role accounts land on **Continue as** to pick the active role; single-role accounts go
straight to their dashboard. Use **Switch** (top bar) to change the active role.

## Troubleshooting

### `PendingModelChangesWarning: The model for context 'AppDbContext' has pending changes`
Your migration/snapshot is older than the current entity model (e.g. after pulling new changes).
Add a migration that captures the changes, then run again:
```bash
dotnet ef migrations add <DescriptiveName> -p src/PralPer.Infrastructure -s src/PralPer.Web
```
If the database hasn't been created yet and you'd rather start clean, regenerate the initial migration:
```bash
dotnet ef database drop -f -p src/PralPer.Infrastructure -s src/PralPer.Web
# delete src/PralPer.Infrastructure/Migrations
dotnet ef migrations add InitialCreate -p src/PralPer.Infrastructure -s src/PralPer.Web
dotnet run --project src/PralPer.Web
```
**Rule:** every change to entities or `AppDbContext` needs its own migration.

## Notes
- Phase 0 screens render via **static server-side rendering**; interactivity (live weight-sum on
  Goal Submission, 360° sliders, dialogs) is enabled per-page (`@rendermode InteractiveServer`)
  as those Employee screens are built.
- When the Database team provides production tables/SPs, map them in **Infrastructure** only;
  Domain/Application/Web are unaffected.
