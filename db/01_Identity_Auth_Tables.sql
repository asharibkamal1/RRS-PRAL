/* =====================================================================================
   PRAL PER 2026 — ASP.NET Core Identity (Authentication / Authorization) tables
   -------------------------------------------------------------------------------------
   Run this ONCE against the same database that holds the DB team's HR_EMPLOYEE / PER_*
   tables. These tables are OWNED BY THE APPLICATION (ASP.NET Core Identity) and are how
   the app does login, password storage and ROLE management (Admin / Manager / Employee).

   They are SEPARATE from HR_EMPLOYEE: HR_EMPLOYEE is the HR master record (no password,
   no login). An app login (AspNetUsers) optionally LINKS to an employee through the
   AspNetUsers.EmployeeId column, which points at HR_EMPLOYEE.EMP_ID.

   When to run this script:
     - Use this only if the application is NOT allowed to run EF Core migrations against
       the production database (i.e. the DBA owns the schema). The DBA runs this once and
       the app then just reads/writes these tables at runtime.
     - If the app IS allowed to migrate, you do not need this file — EF creates these
       tables automatically on first run (see db/README.md, "Path A").

   Safe to re-run: every object is guarded with IF NOT EXISTS.
   Schema matches the EF model exactly (string keys = NVARCHAR(450); custom columns
   AspNetUsers.DisplayName + AspNetUsers.EmployeeId, AspNetRoles.Description).
   ===================================================================================== */

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Roles ---------- */
IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoles] (
        [Id]               NVARCHAR(450)  NOT NULL,
        [Description]      NVARCHAR(MAX)  NULL,         -- custom (ApplicationRole.Description)
        [Name]             NVARCHAR(256)  NULL,
        [NormalizedName]   NVARCHAR(256)  NULL,
        [ConcurrencyStamp] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'RoleNameIndex' AND object_id = OBJECT_ID(N'[dbo].[AspNetRoles]'))
    CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

/* ---------- Users (logins) ---------- */
IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id]                   NVARCHAR(450)     NOT NULL,
        [DisplayName]          NVARCHAR(MAX)     NOT NULL,   -- custom (ApplicationUser.DisplayName)
        [EmployeeId]           INT               NULL,       -- custom: links to HR_EMPLOYEE.EMP_ID
        [UserName]             NVARCHAR(256)     NULL,
        [NormalizedUserName]   NVARCHAR(256)     NULL,
        [Email]                NVARCHAR(256)     NULL,
        [NormalizedEmail]      NVARCHAR(256)     NULL,
        [EmailConfirmed]       BIT               NOT NULL,
        [PasswordHash]         NVARCHAR(MAX)     NULL,
        [SecurityStamp]        NVARCHAR(MAX)     NULL,
        [ConcurrencyStamp]     NVARCHAR(MAX)     NULL,
        [PhoneNumber]          NVARCHAR(MAX)     NULL,
        [PhoneNumberConfirmed] BIT               NOT NULL,
        [TwoFactorEnabled]     BIT               NOT NULL,
        [LockoutEnd]           DATETIMEOFFSET    NULL,
        [LockoutEnabled]       BIT               NOT NULL,
        [AccessFailedCount]    INT               NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'EmailIndex' AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]'))
    CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UserNameIndex' AND object_id = OBJECT_ID(N'[dbo].[AspNetUsers]'))
    CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

/* ---------- Role claims ---------- */
IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims] (
        [Id]         INT            NOT NULL IDENTITY,
        [RoleId]     NVARCHAR(450)  NOT NULL,
        [ClaimType]  NVARCHAR(MAX)  NULL,
        [ClaimValue] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]'))
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims] ([RoleId]);
GO

/* ---------- User claims ---------- */
IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims] (
        [Id]         INT            NOT NULL IDENTITY,
        [UserId]     NVARCHAR(450)  NOT NULL,
        [ClaimType]  NVARCHAR(MAX)  NULL,
        [ClaimValue] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]'))
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims] ([UserId]);
GO

/* ---------- External logins ---------- */
IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins] (
        [LoginProvider]       NVARCHAR(450)  NOT NULL,
        [ProviderKey]         NVARCHAR(450)  NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX)  NULL,
        [UserId]              NVARCHAR(450)  NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]'))
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins] ([UserId]);
GO

/* ---------- User <-> Role join (this is what gives a login its role) ---------- */
IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]'))
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles] ([RoleId]);
GO

/* ---------- User tokens ---------- */
IF OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens] (
        [UserId]        NVARCHAR(450)  NOT NULL,
        [LoginProvider] NVARCHAR(450)  NOT NULL,
        [Name]          NVARCHAR(450)  NOT NULL,
        [Value]         NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

/* ---------- Data Protection key ring ----------
   The app stores the cookie-encryption keys here so a load-balanced (multi-instance)
   deployment can share auth cookies. Required by PralPer (Program.cs PersistKeysToDbContext). */
IF OBJECT_ID(N'[dbo].[DataProtectionKeys]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DataProtectionKeys] (
        [Id]           INT            NOT NULL IDENTITY,
        [FriendlyName] NVARCHAR(MAX)  NULL,
        [Xml]          NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_DataProtectionKeys] PRIMARY KEY ([Id])
    );
END;
GO

/* ---------- OPTIONAL: enforce the HR_EMPLOYEE link at the DB level ----------
   Uncomment if you want the database to guarantee every linked login points at a real
   employee. EMP_ID is BIGINT in HR_EMPLOYEE, so AspNetUsers.EmployeeId (INT) would need
   to be widened to BIGINT first (see db/README.md, "EmployeeId type" note).

   ALTER TABLE [dbo].[AspNetUsers]
       ADD CONSTRAINT [FK_AspNetUsers_HR_EMPLOYEE_EmployeeId]
       FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[HR_EMPLOYEE] ([EMP_ID]);
*/
GO
