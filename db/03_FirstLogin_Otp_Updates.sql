/* =====================================================================================
   PRAL PER — Schema delta for the first-login OTP / forced-password-reset feature
   -------------------------------------------------------------------------------------
   Apply this to the TEST/PROD database to bring it in line with the two new EF Core
   migrations added in this round:
       20260610165304_AddMustChangePassword
       20260612042313_AddOtpLoginFields

   These migrations add COLUMNS and an INDEX only — NO new tables are created.
   Targets the application-owned tables [Users] and [Employees].

   Run order: any time after the Identity tables and the Employees table exist
   (see db/01_Identity_Auth_Tables.sql for the Identity tables).

   Safe to re-run: every change is guarded, so running twice does nothing the 2nd time.
   ===================================================================================== */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ------------------------------------------------------------------ Users
   New columns for the forced first-login password reset + emailed OTP. */

-- Forced password reset on first login (provisioned accounts start = 1).
IF COL_LENGTH(N'[dbo].[Users]', N'MustChangePassword') IS NULL
    ALTER TABLE [dbo].[Users]
        ADD [MustChangePassword] BIT NOT NULL
        CONSTRAINT [DF_Users_MustChangePassword] DEFAULT (0);
GO

-- One-time-password: SHA-256 hash of the current code (never the plaintext).
IF COL_LENGTH(N'[dbo].[Users]', N'OtpCodeHash') IS NULL
    ALTER TABLE [dbo].[Users] ADD [OtpCodeHash] NVARCHAR(MAX) NULL;
GO

-- When the current OTP expires.
IF COL_LENGTH(N'[dbo].[Users]', N'OtpExpiresAtUtc') IS NULL
    ALTER TABLE [dbo].[Users] ADD [OtpExpiresAtUtc] DATETIMEOFFSET NULL;
GO

-- Failed OTP attempts for the current code (locks past the configured max).
IF COL_LENGTH(N'[dbo].[Users]', N'OtpFailedAttempts') IS NULL
    ALTER TABLE [dbo].[Users]
        ADD [OtpFailedAttempts] INT NOT NULL
        CONSTRAINT [DF_Users_OtpFailedAttempts] DEFAULT (0);
GO

-- Set once the OTP has been passed; gates the create-password screen. Cleared after reset.
IF COL_LENGTH(N'[dbo].[Users]', N'OtpVerifiedAtUtc') IS NULL
    ALTER TABLE [dbo].[Users] ADD [OtpVerifiedAtUtc] DATETIMEOFFSET NULL;
GO

/* ------------------------------------------------------------------ Employees
   WorkEmail is now the login id, so it must be (a) indexable -> narrow it from
   NVARCHAR(MAX) to NVARCHAR(450), and (b) unique when present. */

-- (a) Narrow WorkEmail to NVARCHAR(450) only if it is still NVARCHAR(MAX) (max_length = -1).
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE [object_id] = OBJECT_ID(N'[dbo].[Employees]')
      AND [name] = N'WorkEmail'
      AND [max_length] = -1                    -- -1 == NVARCHAR(MAX)
)
BEGIN
    -- Drop any default constraint on the column first (none expected, but be safe).
    DECLARE @df sysname;
    SELECT @df = d.[name]
    FROM sys.default_constraints d
    JOIN sys.columns c ON c.[default_object_id] = d.[object_id]
    WHERE d.[parent_object_id] = OBJECT_ID(N'[dbo].[Employees]') AND c.[name] = N'WorkEmail';
    IF @df IS NOT NULL EXEC(N'ALTER TABLE [dbo].[Employees] DROP CONSTRAINT [' + @df + N'];');

    ALTER TABLE [dbo].[Employees] ALTER COLUMN [WorkEmail] NVARCHAR(450) NULL;
END
GO

-- (b) Unique index on WorkEmail (filtered: allows many NULLs, blocks duplicate emails).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_Employees_WorkEmail'
      AND [object_id] = OBJECT_ID(N'[dbo].[Employees]')
)
    CREATE UNIQUE INDEX [IX_Employees_WorkEmail]
        ON [dbo].[Employees] ([WorkEmail])
        WHERE [WorkEmail] IS NOT NULL;
GO
