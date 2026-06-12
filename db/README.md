# Database integration — DB-team schema + ASP.NET Core Identity

This folder explains how the PRAL PER app uses the **Database team's tables/SPs**
(`HR_EMPLOYEE`, `PER_*`, `sp_Insert_*`) together with **ASP.NET Core Identity**
(login, password, roles).

## The key idea: two groups of tables, one database

| Group | Tables | Who owns / creates them | How the app uses them |
|---|---|---|---|
| **Auth (app-owned)** | `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims`, `DataProtectionKeys` | The **application** (ASP.NET Core Identity). | Login, password hashing, role/claim checks, the auth cookie. |
| **Business (DB-team-owned)** | `HR_EMPLOYEE`, `PER_EVALUATION_PERIOD`, `PER_GOAL`, `PER_PEER_RATING`, `PER_FINAL_RESULT`, … | The **DB team's script** + their `sp_Insert_*` SPs. | Read employees/periods/results; insert raw inputs via SPs. |

They live **side by side in the same database**. Identity does **not** store passwords in
`HR_EMPLOYEE` — that table has no login columns. An app login links to an employee through
the custom column **`AspNetUsers.EmployeeId` → `HR_EMPLOYEE.EMP_ID`**.

## How Identity actually works (short version)

1. **Authentication** — a user submits email + password. Identity looks up `AspNetUsers`
   by normalized email, verifies the password against `PasswordHash` (PBKDF2 — never plain
   text), and issues an **encrypted auth cookie**. The cookie keys are stored in
   `DataProtectionKeys` so multiple servers share them.
2. **Roles** — a user's roles come from `AspNetUserRoles` (join to `AspNetRoles`). This app
   uses exactly three: **Admin / Manager / Employee**. A user can hold several (e.g. the
   demo `admin@` account is Admin + Employee → the "Continue as" screen).
3. **Authorization** — pages/policies require a role (`Program.cs` → `RequireRole(...)`).
   The "Viewing as" switch is an extra active-role claim added by `ActiveRoleClaimsTransformation`.

None of this needs the HR/PER tables. You only **link** a login to an employee so the app
can show that person's goals, ratings and PER report.

## Does Identity "migrate"? Two ways to create the auth tables

### Path A — let EF create them (recommended when the app may run DDL)
Identity tables are part of the app's EF migrations. On first run the app calls
`Database.MigrateAsync()` (see `DbInitializer`) and creates the `AspNet*` +
`DataProtectionKeys` tables automatically. Point the connection string at the DB team's
database and run the app — EF adds the auth tables next to `HR_EMPLOYEE`/`PER_*` and leaves
the DB-team tables alone. **You don't run any SQL by hand.** This is what "Identity migrates"
means.

### Path B — the DBA creates them from script (when the app may NOT run DDL)
Many production setups don't let the app issue `CREATE TABLE`. In that case **Identity won't
migrate**, so create the auth tables once with these scripts and stop the app from migrating:

1. DBA runs, against the DB-team database:
   - **`01_Identity_Auth_Tables.sql`** — creates the 7 `AspNet*` tables + `DataProtectionKeys`
     (idempotent, schema matches the EF model exactly).
   - **`02_Identity_Seed_Roles.sql`** — inserts the Admin/Manager/Employee roles.
2. In the app, **don't auto-migrate**. Either remove the `await db.Database.MigrateAsync();`
   line in `src/PralPer.Infrastructure/Seed/DbInitializer.cs`, or gate it behind a config
   flag (e.g. `Database:AutoMigrate`). The app then just reads/writes the existing tables.
3. Create the actual login accounts through the app's `UserManager` (the `IdentitySeeder`
   on startup, or an admin "create user" page) so passwords are hashed correctly — **never**
   `INSERT` users by hand.

> The scripts here were generated from the project's own EF migrations, so Path A and Path B
> produce the **same** table shapes — you can switch between them safely.

## Connection string

Set `ConnectionStrings:DefaultConnection` in `src/PralPer.Web/appsettings.json` (or, better,
user-secrets / environment variable) to the DB team's server + database, e.g.:

```
Server=YOUR_SQL_HOST;Database=PralPerDb;User Id=app_user;Password=...;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## Employee login (HRMS) — first-login OTP flow

HRMS supplies the **profile**, not a reusable password — so the app **provisions one login
per active employee** and walks each employee through a one-time setup on first sign-in.

**`HrmsUserProvisioner`** (`src/PralPer.Infrastructure/Seed/`) runs at startup and, for every
active employee, creates an ASP.NET Core Identity login with:
- **username/email** = the employee's `WorkEmail` (the login id, also the OTP target). If an
  employee has no work email it synthesizes `{hrcode}@{FallbackEmailDomain}` (configurable).
- **DisplayName** = employee name, **EmployeeId** = link to the HRMS record.
- the **shared default password** (`HrmsProvisioning:DefaultPassword`, default `Pral@12345`).
- **`MustChangePassword = true`**.
- **roles**: `Employee` for everyone; `Manager` too if they are someone's reporting manager.
  (`Admin` is assigned manually — never auto-granted; admin accounts keep
  `MustChangePassword = false` and a real password, so they log in normally.)

It is **idempotent**: existing logins are never re-created and their passwords are never
touched; only missing roles / the employee link are topped up.

### First-login sequence (employees)
1. Employee signs in with **email + the shared default password**.
2. Because `MustChangePassword = true`, the app generates a **6-digit OTP**, stores only its
   **hash + expiry** on the user, and **emails the code** to the employee → **/verify-otp**.
3. Employee enters the OTP. On success an `otp_verified` marker is set → **/set-password**.
4. Employee creates their own password → it's hashed and saved, `MustChangePassword = false`,
   OTP markers cleared → back to the **login** screen.
5. Employee signs in with the new password → dashboard. The OTP / create-password screens are
   **never shown again** (a global middleware only pins users who still have the flag).

OTP rules: 6 digits, 10-minute expiry, single-use, **resend** allowed, locked after 5 wrong
attempts (all configurable under `Otp`).

### Email delivery (OTP)
OTP email goes through **company SMTP** (`Email:Smtp`). If `Host` is blank the code is **logged**
instead of sent, so the flow is fully testable in development without a mail server. Put real
SMTP credentials in user-secrets / environment variables (not committed):

```jsonc
"Email": { "Smtp": {
  "Host": "smtp.yourcompany.com", "Port": 587, "EnableSsl": true,
  "User": "...", "Password": "...", "From": "no-reply@pral.com.pk", "FromName": "PRAL PER"
}},
"Otp": { "Length": 6, "ExpiryMinutes": 10, "MaxAttempts": 5 },
"HrmsProvisioning": {
  "Enabled": true,
  "FallbackEmailDomain": "pral.com.pk",  // when an employee has no WorkEmail; "" = skip them
  "DefaultPassword": "Pral@12345",       // shared first-login password
  "ResyncDefaultPassword": true          // re-apply DefaultPassword on each run to accounts
                                         //   still in first-login (MustChangePassword=true)
}
```

> In production set `FallbackEmailDomain` to "" so only employees with a real work email get a
> login (the OTP must reach a real inbox). Today the provisioner reads the `Employee` entity;
> when it is remapped to `HR_EMPLOYEE` (a `.ToTable("HR_EMPLOYEE")` change in Infrastructure)
> the same provisioner and login flow work unchanged.

## Notes / gaps to confirm with the DB team

- **`EmployeeId` type** — `ApplicationUser.EmployeeId` is `int`, but `HR_EMPLOYEE.EMP_ID` is
  `BIGINT`. Fine while IDs are small, but to add a real FK, widen `AspNetUsers.EmployeeId`
  (and the C# property) to `bigint`/`long`. The optional FK in `01_*.sql` is commented out
  for this reason.
- **Missing lookup tables** — `HR_EMPLOYEE` has `DEPARTMENT_ID` and `DESIGNATION_ID` but the
  DB team's script ships no `DEPARTMENT` / `DESIGNATION` tables. Ask them for those (the app
  currently models `Departments`/`Designations` itself).
- **Mapping the DB-team tables in code** is an Infrastructure-only change (entity
  `.ToTable("HR_EMPLOYEE")` configs + `ExcludeFromMigrations()` so EF never alters them, and
  the `sp_Insert_*` SPs called through the existing `IStoredProcedureExecutor`). The
  auth/login layer in this folder is independent of that work.
