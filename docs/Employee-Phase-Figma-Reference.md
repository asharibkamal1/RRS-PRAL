# Employee Phase — Figma Screen Reference & Findings

> Source: UI/UX team "PRAL-RSS-System-Final-Prototype" — Employee screens (provided as images).
> This is the **final prototype**, so where it conflicts with the CRF text, the Figma is treated as the newer source of truth **pending client confirmation** (see §"Critical Discrepancies").
> Phase 1 scope = **Employee role only**. Foundation must be modular/scalable for Manager & Admin later.

---

## Employee sidebar (navigation)
`Dashboard · My Profile · Goal Submission · Competency Rating · My Evaluations · System Flow`

Top bar: PRAL logo + "Performance Evaluation System", notification bell, **"Viewing as ▾ Employee"** role switcher, user name + email + avatar menu.

---

## Screen E0 — Sign In (shared)
- Logo + "PRAL — Performance Evaluation Report System".
- **Email Address / Employee ID** (mail icon), **Password** (lock icon + show/hide eye).
- **Remember me** · **Forgot Password?**
- Primary **Sign In** button (blue, full width).
- Footer chip: 🔒 "Secure Enterprise Access".

## Screen E0b — "Continue as" (role selection, shared)
- "Welcome back, {Name}. Select your role to continue."
- Role cards (only roles the user holds), each with icon, title, subtitle, **Available** badge, **Continue ›**:
  - **Admin** — "Attribute Administration & Designation Mapping"
  - **Employee** — "Goals, self-evaluation & feedback"
- **← Back to Login**.
- ✅ Confirms **multi-role accounts** + per-session active role + in-app "Viewing as" switch.

## Screen E1 — Employee Dashboard
- **KPI cards (4):** Goal Completion Progress `85%` · Evaluation Status `In Review` · Final PER Score `82.8` · Pending Actions `2`.
- **My Goals Progress:** list of goals, each with weight label + % progress bar (blue).
- **Manager Feedback:** Strengths (green panel), Development Areas (blue panel), "Last updated … / Manager: …".
- **Peer Rating Overview:** Pending Evaluations `5` · Submitted Evaluations `12` · Days Until Deadline `16`.
- **Deadline banner (orange):** "Evaluation Deadline — May 31, 2026 — Complete 5 pending evaluations before deadline".
- **Pending Evaluations** list: name, role • dept, "16 days left" chip, **Evaluate** button.
- **Completed Evaluations** list: name, role • dept, **Submitted** badge, date.

## Screen E2 — My Profile (Employee Profile) — READ ONLY
- **Header card:** avatar, name, designation (blue), **Active** badge, email, phone, "Joined: …".
- **HR Information:** HR Code (e.g. `PRAL-EMP-234`), Accounts Code (`ACC-FIN-089`), Pay Group (`Grade A - Level 3`).
- **Department Details:** Department, Wing, Reporting Manager.
- **Evaluation Period:** Current Period (`Q1 2026 (Jan - Mar)`), Designation, Tenure (`6 years, 4 months`).
- **Promotion History:** vertical timeline (date, "X → Y", note).
- **Increment History:** table — Year | Percentage (green chip) | Amount.
- **Bonus History:** table — Year | Type | Quarter | Amount (green).

## Screen E3 — Goal Submission
- Subtitle: **"Submit Min 3, Max 5 performance goals (Total weight must equal 100%)."**
- **Goal Assessment Weight Summary:** live bar `100% / 100%`; info line "Total Goal Weight = 100%. This will contribute 70% to your Final PER Score."
- **Goal cards (3–5):** Goal Title, Description, **Progress slider (%)**, **Weight %** (numeric stepper), **Rating (0–4)** as 5 radio buttons coloured `4(green) 3(green) 2(amber) 1(orange) 0(red)`, per-row edit/save icons.
- **Submit Goals** button (bottom-right).

## Screen E4 — 360° Competency Rating
- Subtitle: "Peer review evaluation - **10 competency attributes**".
- Blue ratee header card: name, designation • dept, "Evaluating as Peer Reviewer".
- Filters: **Select Rater**, **Select Ratee Department**, **Filter**, **Save**.
- **Ratee List** table: SR | Ratee Employee | Status; rows **expand** to reveal competency groups → attributes with star rating + slider + numeric value + remarks.

## Screen E5 — My Evaluations
- *(Not individually screenshotted in detail — appears to be the list of peer evaluations the employee must complete + their own results. Confirm content.)*

## Screen E6 — Final PER Report (employee can view own)
- **Download PDF** button.
- Blue header: name, designation, HR code, dept, pay group, **Approved** badge, manager.
- **Promotion & Increment History** fields (Recruitment/Last Promotion/Increment/Bonus dates & 1–4 bands).
- **Goal Assessment Score** (e.g. `80%`) — note "Score for each goal 4+3+3+3 = 16, Max total = 5 × 4 = 20".
- **Competency Assessment Score** (e.g. `60%`) — "3+3+2+2+2 = 12, Max 5 × 4 = 20".
- **Final PER Calculation:** Goal Weight `70%`, Competency Weight `30%`, **Final PER Score** (green, e.g. `82.6%` "Excellent").
- **Goal Assessment Details:** per goal — Weight, Progress, Rating, **Contribution = (Weight × Rating) ÷ 4**; bar chart (Score Contribution % vs Goal Weight %).
- **Competency Assessment:** 5 competencies each `x/4` with bars + chart.
- **Manager Remarks:** Strengths, Areas for Development, Overall Comments, Approved by + Date.
- **Approval Status** stepper: Goals Submitted → 360° Evaluation → Manager Review → Final Approval (with dates).

## Screen E7 — System Flow Diagram (read-only)
- **9-step PER workflow:** 1 Admin Configuration → 2 Attribute Setup → 3 Designation Mapping → 4 Assign Rators → 5 Goal Submission → 6 360° Evaluation → 7 PER Calculation → 8 Approval by Manager → 9 Final Approval (HoD).
- **Screen Navigation Map** with routes: `/` `/dashboard` `/profile` `/attributes` `/mapping` `/assign-rators` `/goals` `/evaluation` `/report`.
- **User Roles:** Admin (full), Manager (review & approve PERs), Employee (submit goals & view reports), **Peer Rator** (360° evaluations).
- **Scoring Formula**, **HRMS Integration** list, **Evaluation Cycle Timeline** (Week 1-2 … 7-8).

---

## ⚠️ Critical Discrepancies — Figma (final) vs CRF text

| Topic | CRF text said | Figma (final prototype) shows | Impact |
|---|---|---|---|
| **Rating scale** | 1–10 | **0–4** (radio: 4/3/2/1/0) for both goals & competencies | **Major** — changes scoring engine, validation, UI |
| **Goal count** | Exactly 5 | **Min 3, Max 5** | Validation + data model |
| **Goal weight input** | Decimal 0–1 (sum 1.00) | **Percent 0–100 (sum 100)** | Storage/representation |
| **Progress field** | Not mentioned | **Per-goal Progress %** slider (separate from Rating) | New field on Goal |
| **Goal Score** | Σ(Rating×Weight), 1–10 basis | **Σ((Weight% × Rating) ÷ 4)** → a percentage | Formula |
| **Peer/Competency Score** | Σ(10 ratings)÷10 | **Avg rating ÷ 4** as % (`x/4`) | Formula |
| **Final PER** | (Goal×0.70)+(Peer×0.30) on 1–10 | Same **70/30 split** but on **percentages** | Confirmed concept, new basis |
| **360° attributes** | Designation-driven (your Q1) | Subtitle says "**10 competency attributes**" (fixed?) | Need reconcile |
| **Approval flow** | "no separate manager approval" (your Q7) | Report shows **Manager Review + Final Approval (HoD)** stepper | Need reconcile |

> Note: the Figma's sample numbers are internally inconsistent in places (e.g. report shows both `80%` and `83%` for goal score; final `82.6%` uses `83%×0.7`). The **formulas** above are taken as authoritative; the mockup's arithmetic typos are ignored.

---

## Proposed scoring model (Figma-based, pending confirmation)
```
RatingScale            = 0..4 (integer)
GoalContribution_i (%) = (Weight%_i × Rating_i) / 4
GoalScore (%)          = Σ GoalContribution_i           // weights sum to 100
CompetencyScore (%)    = (Σ attribute/competency ratings) / (count × 4) × 100
FinalPER (%)           = GoalScore × 0.70 + CompetencyScore × 0.30
Colour                 = green ≥ 80 · amber ≥ 60 · red < 60   // (to confirm thresholds on 0–4/% basis)
```

## Open confirmations for client
1. **Rating scale = 0–4** everywhere (replaces CRF's 1–10)? 
2. **Goals = Min 3 / Max 5** (replaces "exactly 5")?
3. **360° attributes:** fixed 10, or designation-driven per ratee (your earlier Q1 = designation-driven)? Reconcile with the "10 competency attributes" label.
4. **Approval workflow:** Figma report shows Manager Review + HoD Final Approval — should Employee Phase render these as read-only status only (approvals built in Manager/Admin phases)?
5. **My Evaluations** screen exact content.
