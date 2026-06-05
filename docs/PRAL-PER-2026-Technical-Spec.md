# PRAL PER 2026 — Technical Specification & Implementation Plan

> **Project:** Performance Evaluation Report (PER) System — FY 2026
> **CR ID:** CRF-HRIS-HRS-4 · **Project ID:** HRIS · **Module:** HRIS-HRS · **Dept:** FBR / PRAL
> **Stack (confirmed):** Blazor — full .NET · SQL Server · EF Core
> **HRMS data (confirmed):** Provided directly in DB tables — **no external API / integration required** (read-only consumption).
> **Status:** Draft v1.0 — written spec for review *before* any code is written.

---

## 1. Purpose & Background

PRAL currently runs employee performance evaluation through a largely **manual** process, causing:
- Inconsistent scoring
- Delays in reporting
- Lack of transparency in the evaluation workflow

**PER 2026** replaces this with a **fully digital, web-based platform** that automates, standardizes, and governs the **annual evaluation cycle for Financial Year 2026**.

**Core scoring model:** `Final Score = 70% Goals + 30% Peer (360°) Competency`.

---

## 2. Scope

### In scope
- 13 functional screens across **3 roles** (Admin, Manager, Employee)
- Admin configuration of Evaluation / Rating / Goal-submission periods
- Competency–attribute repository and designation mapping
- Rator (peer reviewer) assignment
- Goal submission (Section 2) and 360° competency rating (Section 3)
- Automated PER score calculation and consolidated report
- Role-based dashboards and read-only system-flow reference

### Out of scope (this CR)
- HRMS integration / live sync (data is supplied directly in DB tables)
- Payroll, promotion, or increment processing (we only *read* historical bands)
- Mobile native apps

---

## 3. Roles & Access Matrix

| Screen / Capability | Admin | Manager | Employee |
|---|:--:|:--:|:--:|
| Dashboard | ✅ | ✅ | ✅ |
| Employee / My Profile (read-only) | ✅ | ✅ | ✅ |
| Evaluation Period Management | ✅ | — | — |
| Rating Period Management | ✅ | — | — |
| Goal Submission Management (window config) | ✅ | — | — |
| Attribute Administration | ✅ | — | — |
| Attribute → Designation Mapping | ✅ | — | — |
| Assign Rators to Ratee | ✅ | ✅ | — |
| Goal Submission (Section 2) | ✅ | ✅ | ✅ |
| 360° Competency Rating (Section 3) | ✅ | ✅ | ✅ (as assigned rator) |
| PER Report | ✅ | ✅ | view own* |
| System Flow Diagram | ✅ | ✅ | ✅ |

\* Employee visibility of own report — **see open question Q3**.

---

## 4. Screen-by-Screen Functional Spec

> Each screen lists: **fields**, **validation rules**, and **behaviour**. Field types map directly to Blazor components (see §7).

### S1 — Login / Sign-In
- **Fields:** Email Address / Employee ID (required), Password (required), Remember me, Forgot Password link.
- **Rules:** Validate credentials + role; redirect to role-appropriate Dashboard.
- **Behaviour:** Failed login shows inline error; lockout policy via ASP.NET Core Identity (configurable).

### S2 — Dashboard
- **KPI cards (read-only, auto-calculated):** Total Employees, Completed, Pending, Not Started — colour-coded top borders (blue/green/gold/red).
- **Evaluation Progress by Department:** horizontal bar chart, `% = Done ÷ Total × 100`; bar green if > 50%, gold if ≤ 50%.
- **Process Timeline (6 steps):** System Configuration → Designation Mapping → Rator Assignment → Goal Submission → Competency Rating → PER Finalization. Status dots system-driven: Done / Active / Pending.
- **Recent Activity** feed.
- **Buttons:** *View Flow* → System Flow Diagram; *PER Report* → PER Report.
- **Rules:** All values auto-derived from evaluation-status records; no manual input.

### S3 — Employee Profile (Section 1) — READ ONLY
- **Fields (all read-only, from HRMS tables):** Employee Selector (dropdown, required), Employee Name, HR Code, Accounts Code, Department, Wing, Pay Group, Designation, Reporting Manager, Evaluation Period, Recruitment Date, Last Promotion Date, Last Increment Date, Last Increment (1–4), Last Bonus Date, Last Bonus (1–4).
- **Rules:**
  - Employee Selector mandatory; changing it reloads all fields.
  - **Accounts Code = `ACC-` + numeric portion of HR Code** (derived, no override).
  - All fields read-only — no editing.

### S4 — Evaluation Period Management
- **Fields:** Evaluation Start Date (required, date picker), Evaluation End Date (required, date picker), Submit button, Period Grid (Action | Start | End | Status).
- **Rules:**
  - Start ≤ End; both mandatory.
  - No duplicate periods; no overlapping periods.
  - On save: new period = **Active**; previous Active → **Archive**; grid refreshes.
  - Grid `Select` action opens record in view mode.

### S5 — Rating Period Management
- Same pattern as S4 for **Rating Period**.
- **Gate:** Evaluation Period must be **Active** (checked on page load) — else show error + return to Dashboard.

### S6 — Goal Submission Management (window config)
- **Fields:** Goal Submission Start Date (date picker), End Date (date picker), **Allow for Submission** (dropdown, required, enable/disable), **Allow for Department** (selector — restrict to specific depts), **Allow Detail** (selector — restrict to specific employee), Submit, Archive Grid.
- **Rules:**
  - Evaluation Period must be Active (page-load gate).
  - Date format `DD-MMM-YY`; Start ≤ End.
  - *Allow for Submission* must be explicitly selected, else Submit stays disabled.
  - *Allow for Department* mandatory when restricting by department; *Allow Detail* mandatory when restriction is individual.
  - Submit validates `Start ≤ Today ≤ End`; rejects out-of-window submissions.

### S7 — Attribute Administration (Admin only)
- **Filter bar:** Competency filter (dropdown), Attribute search (text, case-insensitive partial), Filter button, Clear button.
- **List table (read-only):** S.No, Competency (chip), Attribute, Weight (0.00–1.00), Edit (✏️), `+ Add Attribute`.
- **Add/Edit form:** Competency (dropdown, required), Attribute Name (text, required), Weight (number, required, 0–1, step 0.05, default 0.20), Save, Back.
- **Rules:**
  - Evaluation Period must be Active (gate).
  - Attribute name not empty; Weight in range.
  - **Advisory:** sum of attribute weights per competency should equal 1.00.
  - Edit pre-populates form; save updates in place.
- **Seed data:** 5 competencies / 20 attributes (see §6).

### S8 — Attribute → Designation Mapping (Admin only)
- **Assignment form:** Competency (dropdown, required), Attribute (dropdown, required), Weight (number, required, 0–1, step 0.05), Designation (dropdown, required), Save, Back.
- **Mapping table (read-only):** S.No, Designation (badge), Competency, Attribute, Weight.
- **Rules:**
  - Evaluation Period must be Active (gate).
  - No duplicate **Designation + Attribute** combination (advisory check).
  - No inline editing — use form to add.

### S9 — Assign Rators to Ratee (Admin / Manager)
- **Selection panel:** Ratee Officer (dropdown, required), Rator Department (dropdown, optional filter), Save, Back.
- **Available rators table:** S.No, Select (checkbox, multi-select), Rator Employee, Department.
- **Rules:**
  - Evaluation Period must be Active (gate).
  - At least one rator selected before save.
  - **Self-rating prevention:** ratee excluded from their own rator list.
  - Save toast: `Rators saved: N assigned`.

### S10 — Goal Submission (Section 2)
- **Header:** Ratee Officer (dropdown, required), Select Goal to Edit (dropdown, quick-jump).
- **Goals table (exactly 5 rows G1–G5):** Goal/Target Description (text, required), Weight (number 0–1, step 0.05), Rating slider (1–10, manager-assessed), Save Goals.
- **Rules:**
  - **Goal submission window must be open.**
  - All 5 descriptions required.
  - **CRITICAL:** sum of all 5 weights **= exactly 1.00**, else block save (live sum check in UI).
  - Rating 1–10 integer (default pre-loaded or 5).
  - Exactly 5 goals — no more, no fewer.
  - **`Goal Score = Σ (Ratingᵢ × Weightᵢ)`**

### S11 — 360° Competency Rating (Section 3)
- **Filter bar:** Rator Employee (dropdown, required), Ratee Department (optional filter), Filter, Save.
- **Ratee list table:** S.No, Ratee Employee, Department, Status badge (Pending/Done/Blank), Expand/Collapse.
- **Expanded panel per ratee:** 10 attribute labels, 10 rating sliders (1–10, default 5), 10 remarks textboxes.
- **Rules:**
  - **Rating Period must be Active** (page-load gate) — else error + return to Dashboard.
  - Only assigned rators appear; **self-rating excluded**.
  - All 10 attributes must be rated for a row to be **Done**; partially rated/unsaved = **Pending**.
  - **`Peer Score = Σ(10 ratings) ÷ 10`**

### S12 — PER Report (read-only)
- **Header:** Employee Selector (dropdown, required).
- **Score cards (circles):** Goals Score (70%), Peer Score (30%), Final PER Score — colour-coded rings.
- **Section 2 — Goals breakdown (×5):** description (truncated 36 chars), rating/10 (coloured), progress bar (fill = rating×10%, blue), weight label (×0.40 etc.).
- **Section 3 — Competency breakdown (×10):** attribute name, peer rating/10, progress bar (fill = rating×10%, green).
- **Rules:**
  - Evaluation Period must be Active (gate).
  - Report generates only for employees with completed goals **AND** ≥ 1 peer rating.
  - Missing data → display `—` or `0.0`.
  - Entirely read-only.

### S13 — System Flow Diagram (read-only)
- Visual map of **Admin Configuration Flow**, **Evaluation Flow**, and **Screen Navigation Map** (reference content from CRF §System Flow).

---

## 5. Business Rules & Formulas (authoritative)

| Rule | Definition |
|---|---|
| **Goal Score** | `Σ (Rating_Gᵢ × Weight_Gᵢ)` for i = 1..5 |
| **Peer Score** | `Σ(10 attribute ratings) ÷ 10` |
| **Final PER Score** | `(Goal Score × 0.70) + (Peer Score × 0.30)` |
| **Score colour** | `≥ 8.0` Green (High) · `≥ 6.0` Amber (Mid) · `< 6.0` Red (Low) |
| **Goal weights** | Exactly 5 goals; weights must sum to **1.00** (block save otherwise) |
| **Period uniqueness** | Only **one Active** Evaluation period and one Active Rating period; previous auto-archived |
| **Period gating** | Most screens verify "Evaluation/Rating Period Active" on page load; else redirect to Dashboard with error |
| **Self-rating** | A person can never be their own rator (excluded in Assign Rators + 360° Rating) |
| **Accounts Code** | `ACC-` + numeric portion of HR Code |
| **Profile** | 100% read-only (sourced from HRMS tables) |

---

## 6. Seed / Reference Data (from CRF)

### Competencies & Attributes (20)
| Competency | Attributes (weight) |
|---|---|
| **Integrity** | Compliance .20, Confidentiality .20, Transparency .20, Ethics .20, Accountability .20 |
| **Team Work & Collaboration** | Collaboration .20, Communication .20, Support .20, Alignment .20, Teamwork .20 |
| **Problem Solving & Innovation** | Analysis .25, Problem-Solving .25, Innovation .25, Improvement .25 |
| **Takes Ownership** | Accountability .33, Commitment .33, Responsibility .34 |
| **Leadership** | Motivation .33, Decision-Making .33, Vision .34 |

### Designations
Manager · Development · Database · Human Resources · Quality Assurance

> **The 10 attributes rated in 360°** (per CRF S11): Compliance, Confidentiality, Transparency, Ethics, Accountability, Collaboration, Communication, Support, Alignment, Teamwork. *(See open question Q1 — fixed 10 vs designation-driven.)*

---

## 7. Proposed Technical Architecture (Blazor / .NET)

### 7.1 Stack
- **.NET 8 Blazor Web App** with **Interactive Server** render mode (recommended): real-time interactivity for sliders & live weight-sum checks, direct DB access, no separate API hosting needed for an internal enterprise tool. *(WASM possible later if offline/scaling needs arise.)*
- **EF Core 8** (code-first migrations) over **SQL Server**.
- **ASP.NET Core Identity** with **role-based authorization** (Admin / Manager / Employee).
- **UI:** MudBlazor **or** Tailwind-styled components to match the polished Annex-A look (charts, KPI cards, sliders, chips, toasts). *(See open question Q2.)*
- **Charts:** ApexCharts.Blazor / Chart.js interop for bar, line, pie.

### 7.2 Layered solution structure
```
PralPer.sln
├── src/
│   ├── PralPer.Domain/          # Entities, enums, value objects, domain rules
│   ├── PralPer.Application/      # Services, DTOs, validators, score calculators
│   ├── PralPer.Infrastructure/  # EF Core DbContext, repositories, migrations, seed
│   └── PralPer.Web/             # Blazor Web App (UI, pages, components, auth)
└── tests/
    └── PralPer.UnitTests/       # Score-formula + validation unit tests
```

### 7.3 Cross-cutting
- **Validation:** FluentValidation in Application layer + Blazor `EditForm` for UI feedback.
- **Period-gate guard:** a reusable component/route guard that checks Active period on load.
- **Audit:** CreatedBy/CreatedAt/ModifiedBy/ModifiedAt on mutable entities (supports Dashboard "Recent Activity").
- **Toasts:** consistent success/error notifications.

---

## 8. Data Model (first draft)

> HRMS-sourced tables are **read-only** to this app. Application-owned tables are read/write.

### HRMS-sourced (read-only) — provided in DB
- **Employee** — `Id, HrCode, AccountsCode, Name, DepartmentId, Wing, PayGroup, DesignationId, ReportingManagerId, RecruitmentDate, LastPromotionDate, LastIncrementDate, LastIncrementBand(1–4), LastBonusDate, LastBonusBand(1–4)`
- **Department** — `Id, Name`
- **Designation** — `Id, Name`  *(Manager, Development, Database, HR, QA)*

### Application-owned (read/write)
- **EvaluationPeriod** — `Id, StartDate, EndDate, Status(Active/Archive), Created*`
- **RatingPeriod** — `Id, EvaluationPeriodId, StartDate, EndDate, Status, Created*`
- **GoalSubmissionWindow** — `Id, EvaluationPeriodId, StartDate, EndDate, AllowSubmission(bool), RestrictDepartmentId?, RestrictEmployeeId?, Status, Created*`
- **Competency** — `Id, Name`
- **Attribute** — `Id, CompetencyId, Name, Weight(decimal), IsActive`
- **DesignationAttributeMap** — `Id, DesignationId, AttributeId, Weight` *(unique: DesignationId+AttributeId)*
- **RatorAssignment** — `Id, EvaluationPeriodId, RateeEmployeeId, RatorEmployeeId` *(unique triple; self-assignment blocked)*
- **Goal** — `Id, EvaluationPeriodId, EmployeeId, GoalNo(1–5), Description, Weight, Rating(1–10)`
- **CompetencyRating** — `Id, RatingPeriodId, RateeEmployeeId, RatorEmployeeId, AttributeId, Rating(1–10), Remarks, Status`
- **PerResult** *(computed/cached)* — `Id, EvaluationPeriodId, EmployeeId, GoalScore, PeerScore, FinalScore, GeneratedAt`

*ER diagram to be added in v1.1.*

---

## 9. Implementation Phases (proposed)

| Phase | Deliverable |
|---|---|
| **0. Foundation** | Solution scaffold, EF Core DbContext, migrations, seed (competencies/attributes/designations), Identity + roles, base layout + sidebar nav |
| **1. Admin Config** | Evaluation Period, Rating Period, Goal Submission window, Attribute Admin, Designation Mapping |
| **2. Assignment & Profiles** | Employee/My Profile (read-only), Assign Rators (with self-exclusion) |
| **3. Evaluation** | Goal Submission (Section 2, live weight-sum), 360° Rating (Section 3, expand/collapse, remarks) |
| **4. Reporting** | PER Report (score circles, breakdowns), Dashboard (KPIs, charts, timeline), System Flow Diagram |
| **5. Hardening** | Period-gate guards everywhere, unit tests for formulas, polish/UX, accessibility, audit feed |

---

## 10. Open Questions (for confirmation)

- **Q1 — 360° attributes:** Are the 10 rated attributes **fixed** (the Integrity + Team Work sets shown in CRF S11), or should they be **driven by the Attribute→Designation mapping** per ratee's designation? The CRF shows a fixed list in S11 but a configurable mapping in S8. *(This changes the rating screen design significantly.)*
- **Q2 — UI component library:** **MudBlazor** (fast, batteries-included, easy charts/sliders/toasts) vs **Tailwind + custom components** (closest pixel match to the mockups, more effort)? Recommendation: **MudBlazor** for speed.
- **Q3 — Employee report visibility:** Can an Employee view **their own** PER Report, or is the report **Admin/Manager-only**? CRF lists PER Report under Manager config but not explicitly under Employee.
- **Q4 — Blazor render mode:** Confirm **Interactive Server** (recommended) vs WASM. Server is simpler for DB-heavy internal apps.
- **Q5 — HRMS table contract:** Please share the **actual schema/column names** of the HRMS tables already in the DB so entities map exactly (names, types, keys).
- **Q6 — Authentication source:** Do users log in via the same HRMS credentials / Active Directory, or do we manage credentials in this app's Identity store?
- **Q7 — Manager goal rating step:** The Evaluation Flow mentions a separate "Manager Goal Rating — line manager reviews and approves goal ratings" step. Is approval a distinct workflow state, or is it the same as setting the rating slider on S10?
- **Q8 — "Recent Activity" & timeline:** Confirm these are derived from audit records we generate (no separate source needed).

---

## 11. Next Step
On your confirmation of the open questions (especially **Q1, Q2, Q5**), I'll proceed to **Phase 0 (Foundation)**: scaffold the Blazor solution, set up EF Core + migrations + seed data, and stand up the base layout and authentication — committed to branch `claude/vigilant-gates-yTBYS`.
