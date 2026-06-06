# PRAL PER 2026 — Technical Specification & Implementation Plan

> **Project:** Performance Evaluation Report (PER) System — FY 2026
> **CR ID:** CRF-HRIS-HRS-4 · **Project ID:** HRIS · **Module:** HRIS-HRS · **Dept:** FBR / PRAL
> **Stack (confirmed):** Blazor — full .NET · **MudBlazor** UI · SQL Server · EF Core
> **HRMS data (confirmed):** Consumed from DB tables — **no external API / integration**. A **local SQL Server DB** is used for development/demo now; the **Database team will later provide the production table schemas**, which we will switch to. Until then we own a local `Employee` table modeled on the HRMS employee profile (see §8).
> **Auth (confirmed):** **App-managed** credentials via ASP.NET Core Identity (not AD/HRMS SSO).
> **Status:** Draft v1.1 — written spec for review *before* any code is written.

### Confirmed decisions (this revision)
- **Q1 — 360° attributes are DESIGNATION-DRIVEN:** each ratee is rated only on the attributes mapped to **their designation** via the Attribute→Designation table (not a fixed list of 10).
- **Q2 — UI library:** **MudBlazor**.
- **Q3 — Employee report visibility:** employees **can view their own** PER report & scores.
- **Q6 — Authentication:** **app-managed** (ASP.NET Core Identity), not AD/SSO.
- **Q7 — Manager goal approval:** **no** separate approval workflow state — the manager simply sets the rating slider on the Goal Submission screen.
- **DB strategy:** local SQL Server for now; swap to DB-team-provided schema when delivered (entities kept in an isolatable Infrastructure layer to ease the swap).

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
- **Expanded panel per ratee:** the attribute sliders + remarks textboxes for the attributes **mapped to that ratee's designation** (grouped by competency, per Annex-A image19). Each slider 1–10, default 5; live numeric display.
- **Rules:**
  - **Rating Period must be Active** (page-load gate) — else error + return to Dashboard.
  - **Designation-driven (Q1):** the attribute set per ratee = `DesignationAttributeMap` rows for the ratee's `DesignationId`. The count varies by role (not a hard-coded 10).
  - Only assigned rators appear; **self-rating excluded**.
  - **All** of the ratee's mapped attributes must be rated for the row to be **Done**; partially rated/unsaved = **Pending**.
  - **`Peer Score = Σ(ratings) ÷ (count of mapped attributes)`** — average across the ratee's designation attributes.

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
| **Peer Score** | `Σ(attribute ratings) ÷ (count of the ratee's designation-mapped attributes)` — average. *(Designation-driven per Q1; CRF examples used 10 because the sample roles mapped 10 attributes.)* |
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
> **Note:** We own a **local** `Employee` table for dev/demo (schema below, modeled on the PRAL HRMS employee profile). When the Database team delivers production schemas, we map to theirs and retire the local definition.

### HRMS-sourced (read-only) — local now, DB-team later

**Employee** — modeled on the PRAL HRMS profile page:
| Column | Type | Notes / Sample |
|---|---|---|
| `Id` | int PK | surrogate |
| `HrCode` | nvarchar | e.g. `3657` |
| `AccountsCode` | nvarchar | derived `ACC-{numeric HrCode}` for PER display |
| `Name` | nvarchar | MUHAMMAD ASHARIB KAMAL |
| `Title` | nvarchar | Mr |
| `JobTitle` / `DesignationId` | FK | SOFTWARE ENGINEER → Designation |
| `DepartmentId` | FK | SOFTWARE DEVELOPMENT |
| `Wing` | nvarchar | DEVELOPMENT (PROVINCIAL REVENUE AUTHORITIES) |
| `EmploymentStatus` | nvarchar | CONTRACTUAL |
| `DateOfBirth` | date | Nov 6, 1993 |
| `Gender` | nvarchar | Male |
| `MaritalStatus` | nvarchar | Married |
| `Cnic` | nvarchar | 61101-6679180-3 |
| `CnicExpiry` | date | Jun 24, 2033 |
| `BloodGroup` | nvarchar | A+ |
| `HouseStreet` | nvarchar | H.NO.226-A |
| `Area` | nvarchar | STREET NO.9, I-14/1 ISLAMABAD |
| `City` / `Province` / `Country` / `ZipCode` | nvarchar | ISLAMABAD / ICT / PAKISTAN / 000000 |
| `MobileNumber` / `TelephoneNumber` | nvarchar | 0315-5896001 |
| `WorkEmail` / `Email` | nvarchar | Asharib.kamal@pral.com.pk |
| `PayGrade` / `PayStep` / `PayGroup` | nvarchar | (PER profile fields) |
| `PostingLocation` | nvarchar | PRAL HEAD QUARTERS |
| `PostingLocationStartDate` / `CurrentStartDate` | date? | nullable (NA) |
| `RmHrCode` | nvarchar | 404 |
| `ReportingManagerId` / `RmName` | FK / nvarchar | MUHAMMAD MUDDASER ABBAS |
| `RecruitmentDate` | date | original joining |
| `LastPromotionDate` | date | |
| `LastIncrementDate` | date | |
| `LastIncrementBand` | int (1–4) | |
| `LastBonusDate` | date | |
| `LastBonusBand` | int (1–4) | |
| `AttendancePercent` | decimal? | % of attendance |

> The PER **Employee Profile screen (S3)** surfaces the CRF-listed subset (HR Code, Accounts Code, Dept, Wing, Pay Group, Designation, Reporting Manager, Evaluation Period, Recruitment/Promotion/Increment/Bonus dates & bands). Remaining columns are stored for completeness and future reuse.

- **Department** — `Id, Name` *(e.g. Software Development, Database, Quality Assurance, Human Resources)*
- **Designation** — `Id, Name` *(Manager, Development, Database, HR, QA, Software Engineer, …)*

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

## 10. Open Questions — status

| # | Question | Resolution |
|---|---|---|
| Q1 | 360 attributes fixed vs designation-driven | **Designation-driven** (mapped per ratee designation) |
| Q2 | UI component library | **MudBlazor** |
| Q3 | Employee can view own report | **Yes** |
| Q4 | Blazor render mode | Proposed **Interactive Server** (please confirm) |
| Q5 | HRMS table contract | Build **local** table now (schema in section 8); swap to DB-team schema later |
| Q6 | Authentication source | **App-managed** (ASP.NET Core Identity) |
| Q7 | Manager goal approval step | **No** separate state — slider rating only |
| Q8 | Recent Activity & timeline source | Assumed derived from our audit records (please confirm) |

### Remaining inputs needed
- **Figma designs:** the proto URL is not machine-readable (403 / client-rendered). Please **export the frames as PNG/PDF** and upload, or share Dev-Mode specs. Until then we build to the **Annex-A mockups** embedded in the CRF.
- **Designation-to-attribute seed mapping:** please confirm which attributes map to each designation. For dev we'll seed a sensible default that you can adjust via Screen S8.

---

## 11. Next Step
With Q1/Q2/Q3/Q5/Q6/Q7 confirmed, I'll proceed to **Phase 0 (Foundation)**: scaffold the .NET 8 Blazor Web App (MudBlazor), set up EF Core + local SQL Server + migrations, seed competencies/attributes/designations + a sample `Employee` (using the provided HRMS profile), and stand up Identity + roles and the base sidebar layout — committed to branch `claude/vigilant-gates-yTBYS`.
