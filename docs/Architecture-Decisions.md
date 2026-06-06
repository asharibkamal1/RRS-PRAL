# Architecture Decisions (running log)

## AD-1 — All scoring/calculation happens in the DATABASE
**Decision:** The application performs **no PER calculations** (Goal Score, Peer Score, Final PER,
percentages, aggregations of scores). All such values are computed by the **database** (stored
procedures) and stored in tables. The app's responsibilities are limited to:
- **Insert** raw inputs (goal weights/progress, peer attribute ratings/scores).
- **Fetch** pre-calculated results/figures and **display** them.

**Implications:**
- Removed the client-side `PerScoreCalculator` and its tests.
- Added `PerResult` and `EmployeeEvaluationSummary` tables holding **pre-calculated** values
  (seeded with **dummy** data for now; produced by the DB team's SPs in production).
- `IDashboardService` / `DashboardService` only **read** (EF queries + DB-side `COUNT`s) — no math.
- `IStoredProcedureExecutor` (Dapper) is the path to call the DB team's calculation/reporting SPs.
- Colour banding of a score (green/amber/red) is treated as **presentation only**, done in the Web layer.

**Swap path:** when the DB team delivers real tables/SPs, change only the Infrastructure
implementations (`DashboardService`, future score services) to call SPs via `IStoredProcedureExecutor`.
Domain / Application / Web remain unchanged.

## AD-2 — Stack & platform
.NET 10 · Blazor (MudBlazor) · EF Core 10 · SQL Server (LocalDB for dev). Clean Architecture
(`Domain → Application → Infrastructure → Web`) with Repository + Unit of Work and a service layer.

## AD-3 — Identity & roles
App-managed ASP.NET Core Identity. Three roles (Admin/Manager/Employee). Multi-role accounts use a
"Continue as" screen and an active-role claim (cookie + `IClaimsTransformation`) with a "Viewing as" switch.

## AD-4 — Phased delivery
Phase 0: foundation + Identity + 3 role dashboards (DB-fetched). Then Employee feature screens,
then Manager, then Admin — all on the same modular base.
