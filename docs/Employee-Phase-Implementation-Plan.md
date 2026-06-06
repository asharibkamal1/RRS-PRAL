# PRAL PER 2026 — Employee Phase Implementation Plan

> **Scope:** Phase 1 = **Employee role** screens only, built on a **modular, scalable, multi-role / claims-based** foundation that Manager and Admin phases plug into later with **no rework**.
> **Stack:** .NET 8 (LTS) · Blazor Web App (Interactive Server) · MudBlazor · EF Core 8 · SQL Server (local for dev).
> **Status:** Planning (no code yet — awaiting go-ahead).

---

## 1. Confirmed decisions driving this plan
- **Roles:** multi-role per user; "Continue as" selection + in-app "Viewing as" switch.
- **Auth:** app-managed ASP.NET Core Identity (no AD/SSO).
- **Rating scale:** **1–10** (goals & competencies).
- **Goals:** **Min 3 / Max 5**; weights as **% summing to 100**; per-goal **Progress %**; Rating 1–10.
- **360°:** **designation-driven** attributes per ratee.
- **Scores (0–10 basis):** `Goal = Σ((Weight%/100)×Rating)` · `Peer = Σ(ratings)/mappedCount` · `Final = Goal×0.70 + Peer×0.30` · colours `≥8 green / ≥6 amber / <6 red`.
- **HRMS:** read from local tables now (Employee modeled on PRAL profile); swap to DB-team schema later.
- **Employee can view own PER report.** Approvals shown read-only in this phase.

---

## 2. Solution structure (modular, scalable)

```
PralPer.sln
├── src/
│   ├── PralPer.Domain/            # Pure C#: entities, enums, value objects, domain services (scoring), no EF/UI deps
│   │   ├── Common/                #   BaseEntity, IAuditable, Result<T>, guard clauses
│   │   ├── Entities/              #   Employee, Goal, CompetencyRating, EvaluationPeriod, ...
│   │   ├── Enums/                 #   PeriodStatus, RatingStatus, RoleName, ...
│   │   └── Scoring/               #   PerScoreCalculator (Goal/Peer/Final), colour bands
│   │
│   ├── PralPer.Application/       # Use-cases, DTOs, interfaces, validators (FluentValidation)
│   │   ├── Abstractions/          #   IAppDbContext, ICurrentUser, IClock, IDatePeriodGuard
│   │   ├── Common/                #   PagedResult, Mapping
│   │   └── Features/              #   Vertical slices (one folder per use-case)
│   │       ├── Dashboard/         #     GetEmployeeDashboard
│   │       ├── Profile/           #     GetMyProfile
│   │       ├── Goals/             #     GetMyGoals, SaveGoals (+ GoalsValidator)
│   │       ├── Competency/        #     GetAssignedRatees, GetRateeAttributes, SaveRatings
│   │       ├── Evaluations/       #     GetMyEvaluations
│   │       └── PerReport/         #     GetPerReport
│   │
│   ├── PralPer.Infrastructure/    # EF Core, Identity, persistence, seeding — swappable
│   │   ├── Persistence/           #   AppDbContext, EntityConfigurations, Migrations
│   │   ├── Identity/              #   ApplicationUser, role/claims setup, IdentitySeeder
│   │   ├── Seed/                  #   Competencies/Attributes/Designations/Employees/sample data
│   │   └── Services/              #   CurrentUser, SystemClock, DatePeriodGuard
│   │
│   └── PralPer.Web/               # Blazor Web App (UI only)
│       ├── Components/
│       │   ├── Layout/            #   MainLayout, NavMenu (role-aware), TopBar (Viewing-as switch)
│       │   ├── Shared/            #   KpiCard, ScoreCircle, WeightSummaryBar, StatusBadge, RatingControl, ProgressSlider, ConfirmDialog
│       │   └── Pages/
│       │       ├── Auth/          #   SignIn, ContinueAs
│       │       └── Employee/      #   Dashboard, MyProfile, GoalSubmission, CompetencyRating, MyEvaluations, SystemFlow, PerReport
│       ├── Auth/                  #   Policies, AuthorizationHandlers, ActiveRoleProvider
│       ├── wwwroot/               #   theme, logo, css
│       └── Program.cs
└── tests/
    └── PralPer.UnitTests/         # Scoring formulas, goal weight/min-max validation, period guard
```

**Why this shape:** Domain has zero framework deps (pure rules + scoring → unit-testable). Application defines interfaces; Infrastructure implements them (so the **DB-team schema swap** is isolated to Infrastructure). Web is thin. Adding Manager/Admin later = new Feature folders + new Pages, **no churn** to existing layers.

---

## 3. Multi-role & claims design (foundation for all phases)

- **Identity model:** `ApplicationUser` ⟷ `AspNetUserRoles` (a user may hold **Admin**, **Manager**, **Employee**, **PeerRator**). Optional `EmployeeId` FK linking a login to their HRMS Employee record.
- **Roles seeded:** `Admin`, `Manager`, `Employee`, `PeerRator`.
- **Active role per session:** after Sign In → **Continue as** lists only the user's roles → chosen role stored as a claim `active_role` (scoped, switchable via top-bar "Viewing as"). Server re-validates the user actually holds that role.
- **Authorization:**
  - Role policies: `EmployeeArea`, `ManagerArea`, `AdminArea`.
  - **Active-role policies** so a multi-role user only sees the area they're "viewing as" (matches the Figma switcher).
  - Route guards + `[Authorize(Policy = ...)]` on pages.
- **Period gating:** reusable `DatePeriodGuard` / `<RequiresActivePeriod>` wrapper for screens that need an Active Evaluation/Rating period.
- **Claims** carry `EmployeeId`, `active_role`, display name → used by `ICurrentUser`.

---

## 4. Data model touched in Employee Phase

Read-only HRMS: **Employee, Department, Designation** (§8 of main spec).
App-owned (created/seeded now, written by Employee actions):
- **EvaluationPeriod / RatingPeriod / GoalSubmissionWindow** — seeded with an Active FY2026 period so Employee screens are usable.
- **Competency, Attribute, DesignationAttributeMap** — seeded (20 attributes + designation mappings) so 360° has data.
- **RatorAssignment** — seeded so the logged-in employee has ratees to evaluate (drives Dashboard + Competency + My Evaluations).
- **Goal** — written by Goal Submission (Title, Description, ProgressPercent, WeightPercent, Rating).
- **CompetencyRating** — written by 360° rating (per designation-mapped attribute + remarks + status).
- **PerResult** — computed for PER Report.
- **(Phase-later)** ManagerFeedback / Approval entities — **read-only placeholders** now so Dashboard "Manager Feedback" and PER "Approval Status" render; populated in Manager phase.

---

## 5. Employee screens → build checklist

| # | Route | Page | Key components | Data/use-case |
|---|---|---|---|---|
| E0 | `/` | SignIn | EditForm, MudTextField | Identity sign-in |
| E0b | `/continue` | ContinueAs | Role cards | user roles → set `active_role` |
| E1 | `/dashboard` | Dashboard | KpiCard×4, goal progress list, ManagerFeedback panel, peer-overview cards, deadline banner, pending/completed lists | GetEmployeeDashboard |
| E2 | `/profile` | MyProfile | header card, info cards, timeline, history tables | GetMyProfile (read-only) |
| E3 | `/goals` | GoalSubmission | WeightSummaryBar, GoalCard×(3–5), ProgressSlider, WeightStepper, RatingControl(1–10) | GetMyGoals / SaveGoals (+validator: 3–5, Σ=100) |
| E4 | `/evaluation` | CompetencyRating | ratee header, filters, ratee table, expandable per-ratee attribute panel (designation-driven), remarks | GetAssignedRatees / GetRateeAttributes / SaveRatings (period gate) |
| E5 | `/my-evaluations` | MyEvaluations | tabs: "To complete" (as rator) + "My results" | GetMyEvaluations |
| E6 | `/report` | PerReport | ScoreCircle×3, goal breakdown + chart, competency breakdown + chart, manager remarks (read-only), approval stepper (read-only), Download PDF | GetPerReport |
| E7 | `/system-flow` | SystemFlow | static 9-step workflow + nav map + legend | static |

**Shared UI primitives** (built once, reused by Manager/Admin): `KpiCard`, `ScoreCircle`, `WeightSummaryBar`, `RatingControl` (1–10), `ProgressSlider`, `StatusBadge`, `SectionCard`, `ConfirmDialog`, charts wrapper.

---

## 6. Validation highlights (FluentValidation + UI)
- **Goals:** 3–5 rows; each Title required; WeightPercent 0–100; **Σ WeightPercent = 100** (live bar turns green only at 100); Rating 1–10; Progress 0–100.
- **360°:** all of a ratee's designation-mapped attributes rated (1–10) → row **Done**; partial → **Pending**; **self-rating excluded**; Rating Period must be Active.
- **Profile:** entirely read-only.
- **Window/period gates:** Goal Submission needs open window; rating needs Active Rating Period; report needs completed goals + ≥1 peer rating.

---

## 7. Theming (match Figma)
- MudBlazor custom theme: primary blue `#2563EB`-ish, success green, warning gold, error red; rounded cards, soft shadows, light-grey app background, left sidebar, white top bar.
- PRAL gear/crescent logo asset in `wwwroot`.
- Reusable colour bands for scores (green/amber/red).

---

## 8. Build order (when approved)
1. **0 — Foundation:** solution + projects, EF Core `AppDbContext`, Identity + 4 roles + claims, migrations, seed (periods, competencies/attributes/designations, employees incl. your HRMS sample, rator assignments, sample goals/ratings), MainLayout + role-aware NavMenu + TopBar switcher, theme. → *runnable shell with login + Continue-as.*
2. **E2 Profile** (read-only, simplest real screen) — validates data flow.
3. **E1 Dashboard** (KPIs + lists).
4. **E3 Goal Submission** (the core interactive screen + validators + unit tests).
5. **E4 Competency Rating** (designation-driven, expandable).
6. **E5 My Evaluations** + **E6 PER Report** + **E7 System Flow**.
7. **Hardening:** unit tests (scoring/validation), polish, accessibility, README + run instructions.

Each step = its own commit on `claude/vigilant-gates-yTBYS`.

---

## 9. Still-open (won't block start)
- **My Evaluations** exact content (assumed: "to complete as rator" + "my results").
- Exact **designation→attribute** mapping rows (will seed a sensible default; tunable via Admin S8 later).
- Final **Figma colours/spacing** for pixel polish (will approximate from screenshots, refine on your feedback).
- Whether scores display on **1–10** or as **%** (×10) — will support both, default per your call.

---

## 10. What I need to start Phase 0
A simple "go" — and optionally answers to §9. On "go" I'll scaffold the foundation and the first screens, committing incrementally. (Reminder: no .NET SDK in this container, so you compile/run locally; I'll include a README with exact `dotnet`/VS steps and connection-string setup.)
