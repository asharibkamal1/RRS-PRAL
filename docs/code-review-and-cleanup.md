# Code Review, Cleanup & Hardening Plan

This document records the review of the PRAL PER codebase, what was cleaned up in this pass,
and the larger items that must be done in a build-enabled environment (this CI sandbox has **no
.NET SDK**, so anything that needs to compile or run tests is staged here rather than done blind).

## What's already good (no action needed)
- **Clean layered architecture**: `Domain` → `Application` (interfaces + DTOs) → `Infrastructure`
  (EF Core + service implementations) → `Web` (Blazor). Dependencies point inward.
- **Services + DI everywhere** — there are **no controllers**; every page depends on an
  interface (`IAdminDashboardService`, `IPerReportService`, …) registered in
  `Infrastructure/DependencyInjection.cs`. This already satisfies "use services, not controllers".
- **Read models / DTOs** are separated from entities; pages never touch `DbContext` directly.

## Done in this pass (safe, verifiable by reading)
1. **Centralized calculation logic** into `PralPer.Domain.Scoring.PerScale` (SRP): rating-bucket
   mapping, band labels, percent↔fraction conversion, 70/30 weighted final score, relative-time.
   Removed the duplicated private `Bucket`/`Ago` helpers from `AdminDashboardService`.
2. **Unit tests** for `PerScale` (`tests/PralPer.UnitTests/PerScaleTests.cs`) covering bucket
   boundaries, bands, conversions, weighting, and relative-time formatting.
3. **Removed dead code**: `AdminDashboardSeeder` (the dashboard is now fully live; the seeder was
   no longer called).

## Recommended next (needs the .NET SDK / a SQL database to verify)
Do these one slice at a time with `dotnet build` + `dotnet test` after each.

1. **Regenerate the EF model snapshot.** The admin-dashboard tables were added with a hand-written
   migration, so `ConfigureWarnings(Ignore(PendingModelChangesWarning))` is currently set in
   `DependencyInjection.cs`. Run `dotnet ef migrations add SyncSnapshot`; if it's empty the snapshot
   already matches and the warning suppression can be removed. Otherwise the unused
   `AdminDashboardStat/SeriesPoint/Activity` tables can be dropped (the dashboard is live now).
2. **Move PER scoring to the database** (your "all calculations on DB side" goal). The seed data
   already treats `PerResult`/`PerReport` as *pre-calculated results* ("in production these come
   from DB stored procedures"). Implement:
   - SPs: `usp_CalculateGoalScore`, `usp_CalculatePeerScore`, `usp_CalculateFinalPer(@employeeId,@periodId)`.
   - The app inserts raw inputs (manager goal ratings, peer attribute ratings) and **reads** the
     computed result — no scoring in C#. Front-end keeps only presentation math (`PerScale`).
3. **Replace seeded demo data with referential inserts** where you want "real" numbers
   (`AdminDemoDataSeeder` already inserts into real tables). Keep seeders idempotent.
4. **Broaden test coverage** with an EF Core **SQLite in-memory** fixture (add `Microsoft.EntityFrameworkCore.Sqlite`
   + project refs to Application/Infrastructure in the test project) and cover:
   - `CatalogAdminService` weight ↔ fraction round-trip and 100%-total validation.
   - `AdminPeriodService` single-active invariant (create/activate archives the rest).
   - `AssignRaterService` save = add/remove diff.
   - `ManagerGoalService.SaveAssessmentAsync` weight-total guard.

## Minor issues noted (low priority)
- Department chip colors use `string.GetHashCode()` (`AssignRaters`, `AdminDashboard`,
  `AdminGoalAdministration`) — stable within a run but can change across restarts. Consider a fixed
  name→color map for consistency.
- `IDashboardService.GetAdminDashboardAsync` + the old minimal `AdminDashboardDto` are now unused
  (superseded by `IAdminDashboardService`). Safe to delete once the build confirms no references.
- Seeder ordering: `AdminDemoDataSeeder` runs before `ManagerPerReportsSeeder`, which then early-exits
  (report count > 1), so a few legacy base employees have no PER report. Intentional/!harmful, but
  worth a comment.
