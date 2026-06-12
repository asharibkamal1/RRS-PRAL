/* =====================================================================================
   PRAL PER — FULL application database schema (all EF Core migrations, in order).
   Generated from the app's migrations. Use this to build the entire app-owned schema
   on an EMPTY database (test/prod) when you cannot run `dotnet ef database update`.

   Creates every application table (AspNet Identity tables, Employees, Departments,
   Goals, PER_* equivalents, dashboards, DataProtectionKeys, ...) plus the new
   first-login OTP columns and the unique WorkEmail index.

   NOTE: this is the APP's own schema. The Database team's HR_EMPLOYEE / PER_* production
   tables are separate (mapped later in Infrastructure). Run on a fresh DB only — it is
   NOT guarded for re-runs. For an existing DB that only needs the latest changes, use
   db/03_FirstLogin_Otp_Updates.sql instead.
   ===================================================================================== */

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [EmployeeId] int NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Competencies] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    CONSTRAINT [PK_Competencies] PRIMARY KEY ([Id])
);

CREATE TABLE [Departments] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
);

CREATE TABLE [Designations] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    CONSTRAINT [PK_Designations] PRIMARY KEY ([Id])
);

CREATE TABLE [EvaluationPeriods] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_EvaluationPeriods] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Attributes] (
    [Id] int NOT NULL IDENTITY,
    [CompetencyId] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Weight] decimal(5,4) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Attributes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attributes_Competencies_CompetencyId] FOREIGN KEY ([CompetencyId]) REFERENCES [Competencies] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Employees] (
    [Id] int NOT NULL IDENTITY,
    [HrCode] nvarchar(50) NOT NULL,
    [AccountsCode] nvarchar(50) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Title] nvarchar(max) NULL,
    [DepartmentId] int NOT NULL,
    [DesignationId] int NOT NULL,
    [Wing] nvarchar(max) NULL,
    [PayGroup] nvarchar(max) NULL,
    [PayGrade] nvarchar(max) NULL,
    [PayStep] nvarchar(max) NULL,
    [EmploymentStatus] nvarchar(max) NULL,
    [PostingLocation] nvarchar(max) NULL,
    [ReportingManagerId] int NULL,
    [RmHrCode] nvarchar(max) NULL,
    [RmName] nvarchar(max) NULL,
    [DateOfBirth] datetime2 NULL,
    [Gender] nvarchar(max) NULL,
    [MaritalStatus] nvarchar(max) NULL,
    [Cnic] nvarchar(max) NULL,
    [CnicExpiry] datetime2 NULL,
    [BloodGroup] nvarchar(max) NULL,
    [WorkEmail] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [MobileNumber] nvarchar(max) NULL,
    [TelephoneNumber] nvarchar(max) NULL,
    [HouseStreet] nvarchar(max) NULL,
    [Area] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [Province] nvarchar(max) NULL,
    [Country] nvarchar(max) NULL,
    [ZipCode] nvarchar(max) NULL,
    [RecruitmentDate] datetime2 NULL,
    [LastPromotionDate] datetime2 NULL,
    [LastIncrementDate] datetime2 NULL,
    [LastIncrementBand] int NULL,
    [LastBonusDate] datetime2 NULL,
    [LastBonusBand] int NULL,
    [AttendancePercent] decimal(5,2) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Designations_DesignationId] FOREIGN KEY ([DesignationId]) REFERENCES [Designations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Employees_ReportingManagerId] FOREIGN KEY ([ReportingManagerId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RatingPeriods] (
    [Id] int NOT NULL IDENTITY,
    [EvaluationPeriodId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_RatingPeriods] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RatingPeriods_EvaluationPeriods_EvaluationPeriodId] FOREIGN KEY ([EvaluationPeriodId]) REFERENCES [EvaluationPeriods] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DesignationAttributeMaps] (
    [Id] int NOT NULL IDENTITY,
    [DesignationId] int NOT NULL,
    [AttributeId] int NOT NULL,
    [Weight] decimal(5,4) NOT NULL,
    CONSTRAINT [PK_DesignationAttributeMaps] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DesignationAttributeMaps_Attributes_AttributeId] FOREIGN KEY ([AttributeId]) REFERENCES [Attributes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DesignationAttributeMaps_Designations_DesignationId] FOREIGN KEY ([DesignationId]) REFERENCES [Designations] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Goals] (
    [Id] int NOT NULL IDENTITY,
    [EvaluationPeriodId] int NOT NULL,
    [EmployeeId] int NOT NULL,
    [GoalNo] int NOT NULL,
    [Title] nvarchar(250) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [ProgressPercent] decimal(5,2) NOT NULL,
    [WeightPercent] decimal(5,2) NOT NULL,
    [Rating] int NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Goals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Goals_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Goals_EvaluationPeriods_EvaluationPeriodId] FOREIGN KEY ([EvaluationPeriodId]) REFERENCES [EvaluationPeriods] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [GoalSubmissionWindows] (
    [Id] int NOT NULL IDENTITY,
    [EvaluationPeriodId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [AllowSubmission] bit NOT NULL,
    [RestrictDepartmentId] int NULL,
    [RestrictEmployeeId] int NULL,
    [Status] int NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_GoalSubmissionWindows] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GoalSubmissionWindows_Departments_RestrictDepartmentId] FOREIGN KEY ([RestrictDepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GoalSubmissionWindows_Employees_RestrictEmployeeId] FOREIGN KEY ([RestrictEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_GoalSubmissionWindows_EvaluationPeriods_EvaluationPeriodId] FOREIGN KEY ([EvaluationPeriodId]) REFERENCES [EvaluationPeriods] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PerResults] (
    [Id] int NOT NULL IDENTITY,
    [EvaluationPeriodId] int NOT NULL,
    [EmployeeId] int NOT NULL,
    [GoalScore] decimal(5,2) NOT NULL,
    [PeerScore] decimal(5,2) NOT NULL,
    [FinalScore] decimal(5,2) NOT NULL,
    [GeneratedAtUtc] datetimeoffset NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_PerResults] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PerResults_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PerResults_EvaluationPeriods_EvaluationPeriodId] FOREIGN KEY ([EvaluationPeriodId]) REFERENCES [EvaluationPeriods] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RatorAssignments] (
    [Id] int NOT NULL IDENTITY,
    [EvaluationPeriodId] int NOT NULL,
    [RateeEmployeeId] int NOT NULL,
    [RatorEmployeeId] int NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_RatorAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RatorAssignments_Employees_RateeEmployeeId] FOREIGN KEY ([RateeEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RatorAssignments_Employees_RatorEmployeeId] FOREIGN KEY ([RatorEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RatorAssignments_EvaluationPeriods_EvaluationPeriodId] FOREIGN KEY ([EvaluationPeriodId]) REFERENCES [EvaluationPeriods] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CompetencyRatings] (
    [Id] int NOT NULL IDENTITY,
    [RatingPeriodId] int NOT NULL,
    [RateeEmployeeId] int NOT NULL,
    [RatorEmployeeId] int NOT NULL,
    [AttributeId] int NOT NULL,
    [Rating] int NOT NULL,
    [Remarks] nvarchar(1000) NULL,
    [Status] int NOT NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CompetencyRatings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CompetencyRatings_Attributes_AttributeId] FOREIGN KEY ([AttributeId]) REFERENCES [Attributes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CompetencyRatings_Employees_RateeEmployeeId] FOREIGN KEY ([RateeEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CompetencyRatings_Employees_RatorEmployeeId] FOREIGN KEY ([RatorEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CompetencyRatings_RatingPeriods_RatingPeriodId] FOREIGN KEY ([RatingPeriodId]) REFERENCES [RatingPeriods] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_Attributes_CompetencyId] ON [Attributes] ([CompetencyId]);

CREATE UNIQUE INDEX [IX_Competencies_Name] ON [Competencies] ([Name]);

CREATE INDEX [IX_CompetencyRatings_AttributeId] ON [CompetencyRatings] ([AttributeId]);

CREATE INDEX [IX_CompetencyRatings_RateeEmployeeId] ON [CompetencyRatings] ([RateeEmployeeId]);

CREATE UNIQUE INDEX [IX_CompetencyRatings_RatingPeriodId_RateeEmployeeId_RatorEmployeeId_AttributeId] ON [CompetencyRatings] ([RatingPeriodId], [RateeEmployeeId], [RatorEmployeeId], [AttributeId]);

CREATE INDEX [IX_CompetencyRatings_RatorEmployeeId] ON [CompetencyRatings] ([RatorEmployeeId]);

CREATE UNIQUE INDEX [IX_Departments_Name] ON [Departments] ([Name]);

CREATE INDEX [IX_DesignationAttributeMaps_AttributeId] ON [DesignationAttributeMaps] ([AttributeId]);

CREATE UNIQUE INDEX [IX_DesignationAttributeMaps_DesignationId_AttributeId] ON [DesignationAttributeMaps] ([DesignationId], [AttributeId]);

CREATE UNIQUE INDEX [IX_Designations_Name] ON [Designations] ([Name]);

CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);

CREATE INDEX [IX_Employees_DesignationId] ON [Employees] ([DesignationId]);

CREATE UNIQUE INDEX [IX_Employees_HrCode] ON [Employees] ([HrCode]);

CREATE INDEX [IX_Employees_ReportingManagerId] ON [Employees] ([ReportingManagerId]);

CREATE INDEX [IX_Goals_EmployeeId] ON [Goals] ([EmployeeId]);

CREATE UNIQUE INDEX [IX_Goals_EvaluationPeriodId_EmployeeId_GoalNo] ON [Goals] ([EvaluationPeriodId], [EmployeeId], [GoalNo]);

CREATE INDEX [IX_GoalSubmissionWindows_EvaluationPeriodId] ON [GoalSubmissionWindows] ([EvaluationPeriodId]);

CREATE INDEX [IX_GoalSubmissionWindows_RestrictDepartmentId] ON [GoalSubmissionWindows] ([RestrictDepartmentId]);

CREATE INDEX [IX_GoalSubmissionWindows_RestrictEmployeeId] ON [GoalSubmissionWindows] ([RestrictEmployeeId]);

CREATE INDEX [IX_PerResults_EmployeeId] ON [PerResults] ([EmployeeId]);

CREATE UNIQUE INDEX [IX_PerResults_EvaluationPeriodId_EmployeeId] ON [PerResults] ([EvaluationPeriodId], [EmployeeId]);

CREATE INDEX [IX_RatingPeriods_EvaluationPeriodId] ON [RatingPeriods] ([EvaluationPeriodId]);

CREATE UNIQUE INDEX [IX_RatorAssignments_EvaluationPeriodId_RateeEmployeeId_RatorEmployeeId] ON [RatorAssignments] ([EvaluationPeriodId], [RateeEmployeeId], [RatorEmployeeId]);

CREATE INDEX [IX_RatorAssignments_RateeEmployeeId] ON [RatorAssignments] ([RateeEmployeeId]);

CREATE INDEX [IX_RatorAssignments_RatorEmployeeId] ON [RatorAssignments] ([RatorEmployeeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260606064705_InitialCreate', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [EmployeeEvaluationSummaries] (
    [Id] int NOT NULL IDENTITY,
    [EvaluationPeriodId] int NOT NULL,
    [EmployeeId] int NOT NULL,
    [GoalCompletionPercent] decimal(5,2) NOT NULL,
    [EvaluationStatus] nvarchar(50) NOT NULL,
    [FinalPerScore] decimal(5,2) NOT NULL,
    [PendingActions] int NOT NULL,
    [PendingEvaluations] int NOT NULL,
    [SubmittedEvaluations] int NOT NULL,
    [DaysUntilDeadline] int NOT NULL,
    [EvaluationDeadline] datetime2 NULL,
    [ManagerStrengths] nvarchar(1000) NULL,
    [ManagerDevelopmentAreas] nvarchar(1000) NULL,
    [ManagerName] nvarchar(200) NULL,
    [FeedbackUpdatedOn] datetime2 NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_EmployeeEvaluationSummaries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmployeeEvaluationSummaries_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EmployeeEvaluationSummaries_EvaluationPeriods_EvaluationPeriodId] FOREIGN KEY ([EvaluationPeriodId]) REFERENCES [EvaluationPeriods] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_EmployeeEvaluationSummaries_EmployeeId] ON [EmployeeEvaluationSummaries] ([EmployeeId]);

CREATE UNIQUE INDEX [IX_EmployeeEvaluationSummaries_EvaluationPeriodId_EmployeeId] ON [EmployeeEvaluationSummaries] ([EvaluationPeriodId], [EmployeeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260606071252_AddEmployeeEvaluationSummary', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Employees] ADD [JobTitle] nvarchar(max) NULL;

CREATE TABLE [BonusHistories] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Year] int NOT NULL,
    [BonusType] nvarchar(100) NOT NULL,
    [Quarter] nvarchar(30) NOT NULL,
    [Amount] decimal(12,2) NOT NULL,
    CONSTRAINT [PK_BonusHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BonusHistories_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [IncrementHistories] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Year] int NOT NULL,
    [Percentage] decimal(5,2) NOT NULL,
    [Amount] decimal(12,2) NOT NULL,
    CONSTRAINT [PK_IncrementHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_IncrementHistories_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PromotionHistories] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [EffectiveDate] datetime2 NOT NULL,
    [FromTitle] nvarchar(150) NULL,
    [ToTitle] nvarchar(150) NOT NULL,
    [Note] nvarchar(250) NULL,
    CONSTRAINT [PK_PromotionHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PromotionHistories_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_BonusHistories_EmployeeId] ON [BonusHistories] ([EmployeeId]);

CREATE INDEX [IX_IncrementHistories_EmployeeId] ON [IncrementHistories] ([EmployeeId]);

CREATE INDEX [IX_PromotionHistories_EmployeeId] ON [PromotionHistories] ([EmployeeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260606104031_AddEmployeeHistory', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Attributes] ADD [Description] nvarchar(300) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260606114256_AddAttributeDescription', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [PerReports] (
    [Id] int NOT NULL IDENTITY,
    [EvaluationPeriodId] int NOT NULL,
    [EmployeeId] int NOT NULL,
    [GoalScorePercent] decimal(5,2) NOT NULL,
    [CompetencyScorePercent] decimal(5,2) NOT NULL,
    [GoalWeightPercent] decimal(5,2) NOT NULL,
    [CompetencyWeightPercent] decimal(5,2) NOT NULL,
    [FinalPercent] decimal(5,2) NOT NULL,
    [Band] nvarchar(50) NOT NULL,
    [Approved] bit NOT NULL,
    [ManagerName] nvarchar(200) NULL,
    [Strengths] nvarchar(2000) NULL,
    [DevelopmentAreas] nvarchar(2000) NULL,
    [OverallComments] nvarchar(2000) NULL,
    [ApprovedBy] nvarchar(200) NULL,
    [ApprovedOn] datetime2 NULL,
    [GoalsSubmittedOn] datetime2 NULL,
    [Evaluation360On] datetime2 NULL,
    [ManagerReviewOn] datetime2 NULL,
    [FinalApprovalOn] datetime2 NULL,
    [CreatedAtUtc] datetimeoffset NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [ModifiedAtUtc] datetimeoffset NULL,
    [ModifiedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_PerReports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PerReports_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PerReports_EvaluationPeriods_EvaluationPeriodId] FOREIGN KEY ([EvaluationPeriodId]) REFERENCES [EvaluationPeriods] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PerReportCompetencyLines] (
    [Id] int NOT NULL IDENTITY,
    [PerReportId] int NOT NULL,
    [SortOrder] int NOT NULL,
    [Competency] nvarchar(150) NOT NULL,
    [Score] int NOT NULL,
    [MaxScore] int NOT NULL,
    CONSTRAINT [PK_PerReportCompetencyLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PerReportCompetencyLines_PerReports_PerReportId] FOREIGN KEY ([PerReportId]) REFERENCES [PerReports] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PerReportGoalLines] (
    [Id] int NOT NULL IDENTITY,
    [PerReportId] int NOT NULL,
    [SortOrder] int NOT NULL,
    [Title] nvarchar(250) NOT NULL,
    [WeightPercent] decimal(5,2) NOT NULL,
    [ProgressPercent] decimal(5,2) NOT NULL,
    [Rating] int NOT NULL,
    [MaxRating] int NOT NULL,
    [ContributionPercent] decimal(5,2) NOT NULL,
    CONSTRAINT [PK_PerReportGoalLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PerReportGoalLines_PerReports_PerReportId] FOREIGN KEY ([PerReportId]) REFERENCES [PerReports] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_PerReportCompetencyLines_PerReportId] ON [PerReportCompetencyLines] ([PerReportId]);

CREATE INDEX [IX_PerReportGoalLines_PerReportId] ON [PerReportGoalLines] ([PerReportId]);

CREATE INDEX [IX_PerReports_EmployeeId] ON [PerReports] ([EmployeeId]);

CREATE UNIQUE INDEX [IX_PerReports_EvaluationPeriodId_EmployeeId] ON [PerReports] ([EvaluationPeriodId], [EmployeeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260607043354_AddPerReport', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [DeptPerformances] (
    [Id] int NOT NULL IDENTITY,
    [SortOrder] int NOT NULL,
    [Department] nvarchar(100) NOT NULL,
    [Completed] int NOT NULL,
    [Pending] int NOT NULL,
    CONSTRAINT [PK_DeptPerformances] PRIMARY KEY ([Id])
);

CREATE TABLE [ManagerApprovals] (
    [Id] int NOT NULL IDENTITY,
    [SortOrder] int NOT NULL,
    [EmployeeName] nvarchar(200) NOT NULL,
    [ApprovalType] nvarchar(100) NOT NULL,
    [AgoText] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_ManagerApprovals] PRIMARY KEY ([Id])
);

CREATE TABLE [ManagerDashboardStats] (
    [Id] int NOT NULL IDENTITY,
    [TeamEvaluationProgress] decimal(5,2) NOT NULL,
    [PendingApprovals] int NOT NULL,
    [AverageTeamScore] decimal(5,2) NOT NULL,
    [EmployeesAwaitingReview] int NOT NULL,
    [TotalEvaluationsAssigned] int NOT NULL,
    [PendingEvaluations] int NOT NULL,
    [SubmittedEvaluations] int NOT NULL,
    [DaysUntilDeadline] int NOT NULL,
    CONSTRAINT [PK_ManagerDashboardStats] PRIMARY KEY ([Id])
);

CREATE TABLE [ManagerRaters] (
    [Id] int NOT NULL IDENTITY,
    [SortOrder] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [IsActiveRator] bit NOT NULL,
    CONSTRAINT [PK_ManagerRaters] PRIMARY KEY ([Id])
);

CREATE TABLE [PeerEvaluationSnapshots] (
    [Id] int NOT NULL IDENTITY,
    [RaterId] int NOT NULL,
    [SortOrder] int NOT NULL,
    [RateeName] nvarchar(200) NOT NULL,
    [RateeDepartment] nvarchar(100) NOT NULL,
    [RateeJobTitle] nvarchar(150) NOT NULL,
    [Status] nvarchar(30) NOT NULL,
    [PeriodName] nvarchar(100) NOT NULL,
    [AverageRating] decimal(5,2) NOT NULL,
    [CompetenciesRated] int NOT NULL,
    [AssessmentScorePercent] decimal(5,2) NOT NULL,
    CONSTRAINT [PK_PeerEvaluationSnapshots] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PeerEvaluationSnapshots_ManagerRaters_RaterId] FOREIGN KEY ([RaterId]) REFERENCES [ManagerRaters] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PeerEvaluationLines] (
    [Id] int NOT NULL IDENTITY,
    [SnapshotId] int NOT NULL,
    [SortOrder] int NOT NULL,
    [Competency] nvarchar(150) NOT NULL,
    [AttributeName] nvarchar(200) NOT NULL,
    [Remarks] nvarchar(400) NULL,
    [Score] int NOT NULL,
    [MaxScore] int NOT NULL,
    CONSTRAINT [PK_PeerEvaluationLines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PeerEvaluationLines_PeerEvaluationSnapshots_SnapshotId] FOREIGN KEY ([SnapshotId]) REFERENCES [PeerEvaluationSnapshots] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_PeerEvaluationLines_SnapshotId] ON [PeerEvaluationLines] ([SnapshotId]);

CREATE INDEX [IX_PeerEvaluationSnapshots_RaterId] ON [PeerEvaluationSnapshots] ([RaterId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260607053748_AddManagerDashboard', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [AdminDashboardStats] (
    [Id] int NOT NULL IDENTITY,
    [TotalEmployees] int NOT NULL,
    [EmployeesTrend] decimal(6,2) NOT NULL,
    [EvaluationsPending] int NOT NULL,
    [PendingTrend] decimal(6,2) NOT NULL,
    [CompletedReviews] int NOT NULL,
    [CompletedTrend] decimal(6,2) NOT NULL,
    [ActiveCycleName] nvarchar(100) NOT NULL,
    [ActiveCyclePercent] decimal(6,2) NOT NULL,
    [StatusCompletedPercent] decimal(6,2) NOT NULL,
    [StatusNotStartedPercent] decimal(6,2) NOT NULL,
    [StatusInProgressPercent] decimal(6,2) NOT NULL,
    CONSTRAINT [PK_AdminDashboardStats] PRIMARY KEY ([Id])
);

CREATE TABLE [AdminDashboardSeriesPoints] (
    [Id] int NOT NULL IDENTITY,
    [Category] nvarchar(40) NOT NULL,
    [SortOrder] int NOT NULL,
    [Label] nvarchar(100) NOT NULL,
    [ValueA] decimal(9,2) NOT NULL,
    [ValueB] decimal(9,2) NOT NULL,
    CONSTRAINT [PK_AdminDashboardSeriesPoints] PRIMARY KEY ([Id])
);

CREATE TABLE [AdminActivities] (
    [Id] int NOT NULL IDENTITY,
    [SortOrder] int NOT NULL,
    [ActorName] nvarchar(200) NOT NULL,
    [Description] nvarchar(300) NOT NULL,
    [AgoText] nvarchar(50) NULL,
    CONSTRAINT [PK_AdminActivities] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260607060000_AddAdminDashboard', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AdminActivities]') AND [c].[name] = N'AgoText');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [AdminActivities] DROP CONSTRAINT ' + @var + ';');
UPDATE [AdminActivities] SET [AgoText] = N'' WHERE [AgoText] IS NULL;
ALTER TABLE [AdminActivities] ALTER COLUMN [AgoText] nvarchar(50) NOT NULL;
ALTER TABLE [AdminActivities] ADD DEFAULT N'' FOR [AgoText];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260607111033_SyncSnapshot', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [DataProtectionKeys] (
    [Id] int NOT NULL IDENTITY,
    [FriendlyName] nvarchar(max) NULL,
    [Xml] nvarchar(max) NULL,
    CONSTRAINT [PK_DataProtectionKeys] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260610065635_AddDataProtectionKeys', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [AspNetUsers] ADD [MustChangePassword] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260610165304_AddMustChangePassword', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'WorkEmail');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Employees] ALTER COLUMN [WorkEmail] nvarchar(450) NULL;

ALTER TABLE [AspNetUsers] ADD [OtpCodeHash] nvarchar(max) NULL;

ALTER TABLE [AspNetUsers] ADD [OtpExpiresAtUtc] datetimeoffset NULL;

ALTER TABLE [AspNetUsers] ADD [OtpFailedAttempts] int NOT NULL DEFAULT 0;

ALTER TABLE [AspNetUsers] ADD [OtpVerifiedAtUtc] datetimeoffset NULL;

CREATE UNIQUE INDEX [IX_Employees_WorkEmail] ON [Employees] ([WorkEmail]) WHERE [WorkEmail] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260612042313_AddOtpLoginFields', N'10.0.9');

COMMIT;
GO

