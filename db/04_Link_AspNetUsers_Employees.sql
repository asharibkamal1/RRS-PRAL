/* =====================================================================================
   PRAL PER — Link AspNetUsers to the Employees table
   -------------------------------------------------------------------------------------
   Adds the relationship  AspNetUsers.EmployeeId -> Employees.Id  (the existing app
   Employees table — NOT a separate HR_EMPLOYEE table).

   Mirrors EF migration: 20260612xxxxxx_LinkAspNetUsersToEmployees
     - index    IX_AspNetUsers_EmployeeId
     - FK       FK_AspNetUsers_Employees_EmployeeId  (ON DELETE SET NULL)

   Run order: after both [AspNetUsers] (db/01_Identity_Auth_Tables.sql) and [Employees]
   exist. Safe to re-run (guarded).

   IMPORTANT: every existing AspNetUsers.EmployeeId value must point at a real Employees.Id
   (or be NULL) before the FK is added, otherwise the ALTER will fail. Logins are
   provisioned with valid Employees.Id, so this normally holds.
   ===================================================================================== */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Index on the FK column (created by EF; speeds up the join and the constraint).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE [name] = N'IX_AspNetUsers_EmployeeId'
      AND [object_id] = OBJECT_ID(N'[dbo].[AspNetUsers]')
)
    CREATE INDEX [IX_AspNetUsers_EmployeeId]
        ON [dbo].[AspNetUsers] ([EmployeeId]);
GO

-- Foreign key AspNetUsers.EmployeeId -> Employees.Id (nullable link; SET NULL on delete).
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE [name] = N'FK_AspNetUsers_Employees_EmployeeId'
      AND [parent_object_id] = OBJECT_ID(N'[dbo].[AspNetUsers]')
)
    ALTER TABLE [dbo].[AspNetUsers]
        ADD CONSTRAINT [FK_AspNetUsers_Employees_EmployeeId]
        FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([Id])
        ON DELETE SET NULL;
GO
