/* =====================================================================================
   PRAL PER — Permission catalog table (for the admin permission-management screens)
   -------------------------------------------------------------------------------------
   A simple catalog of permission names the admin assigns to roles (RoleClaims) and users
   (UserClaims, claim type "permission"). Mirrors EF migration AddPermissionCatalog.
   The app seeds a default set on startup; admins manage the rest from the UI.

   Safe to re-run (guarded).
   ===================================================================================== */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[Permissions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Permissions] (
        [Id]          INT            NOT NULL IDENTITY,
        [Name]        NVARCHAR(150)  NOT NULL,
        [Description] NVARCHAR(400)  NULL,
        [IsActive]    BIT            NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Permissions_Name' AND object_id = OBJECT_ID(N'[dbo].[Permissions]'))
    CREATE UNIQUE INDEX [IX_Permissions_Name] ON [dbo].[Permissions] ([Name]);
GO
