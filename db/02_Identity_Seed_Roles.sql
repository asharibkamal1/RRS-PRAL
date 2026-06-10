/* =====================================================================================
   PRAL PER 2026 — Seed the three application roles
   -------------------------------------------------------------------------------------
   Run AFTER 01_Identity_Auth_Tables.sql. Inserts the canonical roles the app expects
   (must match PralPer.Domain.Constants.RoleNames: Admin / Manager / Employee).

   NOTE ON USERS: do NOT insert into AspNetUsers by hand. Passwords must be hashed by
   ASP.NET Core Identity (PBKDF2). Create logins through the app's UserManager (the
   IdentitySeeder already does this on startup, or build an admin "create user" screen).
   This script only seeds ROLES, which are safe to insert directly.

   Safe to re-run.
   ===================================================================================== */

SET NOCOUNT ON;

;WITH roles([Name]) AS (
    SELECT 'Admin'   UNION ALL
    SELECT 'Manager' UNION ALL
    SELECT 'Employee'
)
INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [Description], [ConcurrencyStamp])
SELECT NEWID(), r.[Name], UPPER(r.[Name]), r.[Name] + ' role', NEWID()
FROM roles r
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[AspNetRoles] x WHERE x.[NormalizedName] = UPPER(r.[Name])
);

SELECT [Id], [Name], [NormalizedName] FROM [dbo].[AspNetRoles] ORDER BY [Name];
GO
