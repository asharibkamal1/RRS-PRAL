/* =====================================================================================
   PRAL PER — Link Users to the Employees table
   -------------------------------------------------------------------------------------
   Adds the relationship  Users.EmployeeId -> Employees.Id  (the existing app
   Employees table — NOT a separate HR_EMPLOYEE table).

   Mirrors EF migration: 20260612xxxxxx_LinkAspNetUsersToEmployees
     - index    IX_Users_EmployeeId
     - FK       FK_Users_Employees_EmployeeId  (ON DELETE SET NULL)

   Run order: after both [Users] (db/01_Identity_Auth_Tables.sql) and [Employees]
   exist. Safe to re-run (guarded).

   IMPORTANT: every existing Users.EmployeeId value must point at a real Employees.Id
   (or be NULL) before the FK is added, otherwise the ALTER will fail. Logins are
   provisioned with valid Employees.Id, so this normally holds.
   ===================================================================================== */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Index on the FK column (created by EF; speeds up the join and the constraint).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_Users_EmployeeId'
      AND [object_id] = OBJECT_ID(N'[dbo].[Users]')
)
    CREATE INDEX [IX_Users_EmployeeId]
        ON [dbo].[Users] ([EmployeeId]);
GO

-- Foreign key Users.EmployeeId -> Employees.Id (nullable link; SET NULL on delete).
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE [name] = N'FK_Users_Employees_EmployeeId'
      AND [parent_object_id] = OBJECT_ID(N'[dbo].[Users]')
)
    ALTER TABLE [dbo].[Users]
        ADD CONSTRAINT [FK_Users_Employees_EmployeeId]
        FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([Id])
        ON DELETE SET NULL;
GO
