/* =====================================================================================
   PRAL PER — Identity (Authentication / Authorization) tables
   -------------------------------------------------------------------------------------
   Professional table names (no "AspNet" prefix): Users, Roles, UserRoles, UserClaims,
   UserLogins, UserTokens, RoleClaims — plus DataProtectionKeys.

   These tables are OWNED BY THE APPLICATION (ASP.NET Core Identity) and are how the app
   does login, password storage and ROLE management (Admin / Manager / Employee).

   An app login (Users) optionally LINKS to an employee via Users.EmployeeId -> Employees.Id
   (see db/04_Link_Users_Employees.sql).

   Safe to re-run: every object is guarded.
   ===================================================================================== */

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Roles ---------- */
IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Roles] (
        [Id]               NVARCHAR(450)  NOT NULL,
        [Description]      NVARCHAR(MAX)  NULL,         -- custom (ApplicationRole.Description)
        [Name]             NVARCHAR(256)  NULL,
        [NormalizedName]   NVARCHAR(256)  NULL,
        [ConcurrencyStamp] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'RoleNameIndex' AND object_id = OBJECT_ID(N'[dbo].[Roles]'))
    CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

/* ---------- Users (logins) ---------- */
IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id]                   NVARCHAR(450)     NOT NULL,
        [DisplayName]          NVARCHAR(MAX)     NOT NULL,   -- custom (ApplicationUser.DisplayName)
        [EmployeeId]           INT               NULL,       -- custom: links to Employees.Id
        [MustChangePassword]   BIT               NOT NULL CONSTRAINT [DF_Users_MustChangePassword] DEFAULT (0),  -- forced first-login reset
        [OtpCodeHash]          NVARCHAR(MAX)     NULL,       -- first-login OTP (SHA-256 hash, never plaintext)
        [OtpExpiresAtUtc]      DATETIMEOFFSET    NULL,
        [OtpFailedAttempts]    INT               NOT NULL CONSTRAINT [DF_Users_OtpFailedAttempts] DEFAULT (0),
        [OtpVerifiedAtUtc]     DATETIMEOFFSET    NULL,       -- set once OTP passed; gates create-password
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
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'EmailIndex' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
    CREATE INDEX [EmailIndex] ON [dbo].[Users] ([NormalizedEmail]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UserNameIndex' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
    CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

/* ---------- Role claims ---------- */
IF OBJECT_ID(N'[dbo].[RoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RoleClaims] (
        [Id]         INT            NOT NULL IDENTITY,
        [RoleId]     NVARCHAR(450)  NOT NULL,
        [ClaimType]  NVARCHAR(MAX)  NULL,
        [ClaimValue] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleClaims_Roles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RoleClaims_RoleId' AND object_id = OBJECT_ID(N'[dbo].[RoleClaims]'))
    CREATE INDEX [IX_RoleClaims_RoleId] ON [dbo].[RoleClaims] ([RoleId]);
GO

/* ---------- User claims ---------- */
IF OBJECT_ID(N'[dbo].[UserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserClaims] (
        [Id]         INT            NOT NULL IDENTITY,
        [UserId]     NVARCHAR(450)  NOT NULL,
        [ClaimType]  NVARCHAR(MAX)  NULL,
        [ClaimValue] NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserClaims_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserClaims_UserId' AND object_id = OBJECT_ID(N'[dbo].[UserClaims]'))
    CREATE INDEX [IX_UserClaims_UserId] ON [dbo].[UserClaims] ([UserId]);
GO

/* ---------- External logins ---------- */
IF OBJECT_ID(N'[dbo].[UserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserLogins] (
        [LoginProvider]       NVARCHAR(450)  NOT NULL,
        [ProviderKey]         NVARCHAR(450)  NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX)  NULL,
        [UserId]              NVARCHAR(450)  NOT NULL,
        CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_UserLogins_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserLogins_UserId' AND object_id = OBJECT_ID(N'[dbo].[UserLogins]'))
    CREATE INDEX [IX_UserLogins_UserId] ON [dbo].[UserLogins] ([UserId]);
GO

/* ---------- User <-> Role join (this is what gives a login its role) ---------- */
IF OBJECT_ID(N'[dbo].[UserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserRoles] (
        [UserId] NVARCHAR(450) NOT NULL,
        [RoleId] NVARCHAR(450) NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId]
            FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_UserRoles_RoleId' AND object_id = OBJECT_ID(N'[dbo].[UserRoles]'))
    CREATE INDEX [IX_UserRoles_RoleId] ON [dbo].[UserRoles] ([RoleId]);
GO

/* ---------- User tokens ---------- */
IF OBJECT_ID(N'[dbo].[UserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UserTokens] (
        [UserId]        NVARCHAR(450)  NOT NULL,
        [LoginProvider] NVARCHAR(450)  NOT NULL,
        [Name]          NVARCHAR(450)  NOT NULL,
        [Value]         NVARCHAR(MAX)  NULL,
        CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_UserTokens_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
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
