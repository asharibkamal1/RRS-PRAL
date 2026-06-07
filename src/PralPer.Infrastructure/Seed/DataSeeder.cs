using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>
/// Seeds reference + sample data from the CRF and Figma prototype so the Employee screens are usable:
/// departments, designations, employees, competencies/attributes (20), designation→attribute map,
/// an active evaluation/rating period, an open goal-submission window, rator assignments,
/// and sample goals/peer ratings for the demo employee (Aamir Abdul Aziz).
/// Idempotent: returns early if employees already exist.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Employees.AnyAsync()) return;

        // ---- Departments ----
        var dIt = new Department { Name = "Software Development" };
        var dDb = new Department { Name = "Database" };
        var dQa = new Department { Name = "Quality Assurance" };
        var dHr = new Department { Name = "Human Resources" };
        var dOps = new Department { Name = "Operations" };
        db.Departments.AddRange(dIt, dDb, dQa, dHr, dOps);

        // ---- Designations (drive the 360° attribute mapping) ----
        var gManager = new Designation { Name = "Manager" };
        var gDev = new Designation { Name = "Development" };
        var gDba = new Designation { Name = "Database" };
        var gHr = new Designation { Name = "Human Resources" };
        var gQa = new Designation { Name = "Quality Assurance" };
        db.Designations.AddRange(gManager, gDev, gDba, gHr, gQa);
        await db.SaveChangesAsync();

        // ---- Employees (Ids 1..8 in insert order on a fresh DB) ----
        var emps = new List<Employee>
        {
            New("HR-2024-0145", "Aamir Abdul Aziz",       dIt, gDev,     wing: "Development Wing", pay: "Grade A - Level 3", email: "employee@pral.com.pk", jobTitle: "Senior Software Engineer"),
            New("PRAL-EMP-002", "Abdul Hafeez Butt",      dIt, gManager, wing: "Development Wing", pay: "Grade A - Level 4", email: "manager@pral.com.pk"),
            New("PRAL-EMP-234", "Abdul Wadood Sherani",   dIt, gDev,     wing: "Development Wing", pay: "Grade A - Level 3", email: "admin@pral.com.pk"),
            New("PRAL-EMP-004", "Abdul Rehman",           dIt, gDev,     wing: "Development Wing", pay: "Grade A - Level 2"),
            New("PRAL-EMP-005", "Abbas Ali",              dDb, gDba,     wing: "Operations Wing",  pay: "Grade A - Level 2"),
            New("PRAL-EMP-006", "Abdul Ghafoor",          dOps,gDev,     wing: "Operations Wing",  pay: "Grade A - Level 2"),
            New("PRAL-EMP-007", "Mariam Zafar",           dQa, gQa,      wing: "Quality Wing",     pay: "Grade A - Level 2"),
            New("3657",         "Muhammad Asharib Kamal", dIt, gDev,     wing: "Development (Provincial Revenue Authorities)", pay: "Grade A - Level 1", email: "Asharib.kamal@pral.com.pk"),
        };
        db.Employees.AddRange(emps);
        await db.SaveChangesAsync();

        // reporting manager = Abdul Hafeez Butt (index 1) for the IT devs
        var mgr = emps[1];
        foreach (var e in emps.Where(x => x.DesignationId == gDev.Id))
        {
            e.ReportingManagerId = mgr.Id;
            e.RmName = mgr.Name;
        }
        await db.SaveChangesAsync();

        // ---- Competencies & 10 attributes (360° prototype) ----
        var leadership = new Competency { Name = "Leadership & Innovation" };
        var integrity = new Competency { Name = "Integrity" };
        var communication = new Competency { Name = "Communication" };
        db.Competencies.AddRange(leadership, integrity, communication);
        await db.SaveChangesAsync();

        var attrs = new List<AttributeItem>
        {
            Attr(leadership, "Leadership", 0.34m, "Ability to lead teams and inspire others"),
            Attr(leadership, "Problem Solving", 0.33m, "Analytical thinking and solution finding"),
            Attr(leadership, "Innovation", 0.33m, "Creative thinking and new ideas"),
            Attr(integrity, "Honesty", 0.50m, "Works with dedication"),
            Attr(integrity, "Work Ethics", 0.50m, "Respectful towards peers"),
            Attr(communication, "Communication", 0.20m, "Clear and effective communication"),
            Attr(communication, "Teamwork", 0.20m, "Collaboration and team contribution"),
            Attr(communication, "Time Management", 0.20m, "Efficient use of time and prioritization"),
            Attr(communication, "Adaptability", 0.20m, "Flexibility in changing environments"),
            Attr(communication, "Client Relations", 0.20m, "Building strong client relationships"),
        };
        db.Attributes.AddRange(attrs);
        await db.SaveChangesAsync();

        // ---- Designation→Attribute map: all 10 attributes per designation ----
        var allTen = attrs;
        foreach (var g in new[] { gManager, gDev, gDba, gHr, gQa })
            foreach (var a in allTen)
                db.DesignationAttributeMaps.Add(new DesignationAttributeMap { DesignationId = g.Id, AttributeId = a.Id, Weight = a.Weight });
        await db.SaveChangesAsync();

        // ---- Active evaluation / rating period + open goal window ----
        var period = new EvaluationPeriod
        {
            Name = "Q1 2026 (Jan - Mar)",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Status = PeriodStatus.Active
        };
        db.EvaluationPeriods.Add(period);
        await db.SaveChangesAsync();

        db.RatingPeriods.Add(new RatingPeriod
        {
            EvaluationPeriodId = period.Id,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Status = PeriodStatus.Active
        });
        db.GoalSubmissionWindows.Add(new GoalSubmissionWindow
        {
            EvaluationPeriodId = period.Id,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            AllowSubmission = true,
            Status = PeriodStatus.Active
        });
        await db.SaveChangesAsync();

        // ---- Rator assignments: Aamir (emps[0]) is a peer rator for several ratees ----
        var aamir = emps[0];
        foreach (var ratee in new[] { emps[3], emps[4], emps[6] })
            db.RatorAssignments.Add(new RatorAssignment { EvaluationPeriodId = period.Id, RateeEmployeeId = ratee.Id, RatorEmployeeId = aamir.Id });
        // and others rate Aamir
        foreach (var rator in new[] { emps[1], emps[3], emps[4] })
            db.RatorAssignments.Add(new RatorAssignment { EvaluationPeriodId = period.Id, RateeEmployeeId = aamir.Id, RatorEmployeeId = rator.Id });
        await db.SaveChangesAsync();

        // ---- Sample goals for Aamir (Figma) ----
        var goalData = new (string title, decimal progress, decimal weight, int rating)[]
        {
            ("Complete React Migration Project", 50, 25, 8),
            ("Implement CI/CD Pipeline",        100, 20, 9),
            ("Mentor Junior Developers",         70, 15, 7),
            ("Code Quality Improvements",        90, 30, 7),
            ("Documentation Updates",            80, 10, 8),
        };
        var gn = 1;
        foreach (var (title, progress, weight, rating) in goalData)
        {
            db.Goals.Add(new Goal
            {
                EvaluationPeriodId = period.Id,
                EmployeeId = aamir.Id,
                GoalNo = gn++,
                Title = title,
                ProgressPercent = progress,
                WeightPercent = weight,
                Rating = rating
            });
        }
        await db.SaveChangesAsync();

        // ---- Sample peer ratings for Aamir from manager (all 10 attributes, 0–10 scale) ----
        var ratingPeriod = await db.RatingPeriods.FirstAsync(rp => rp.EvaluationPeriodId == period.Id);
        var sampleScores = new[] { 8, 9, 8, 9, 9, 7, 8, 7, 8, 7 };
        for (var k = 0; k < allTen.Count; k++)
        {
            db.CompetencyRatings.Add(new CompetencyRating
            {
                RatingPeriodId = ratingPeriod.Id,
                RateeEmployeeId = aamir.Id,
                RatorEmployeeId = mgr.Id,
                AttributeId = allTen[k].Id,
                Rating = sampleScores[k],
                Status = RatingStatus.Done
            });
        }
        await db.SaveChangesAsync();

        // ---- Dummy PRE-CALCULATED results (in production these come from DB stored procedures) ----
        db.PerResults.Add(new PerResult
        {
            EvaluationPeriodId = period.Id,
            EmployeeId = aamir.Id,
            GoalScore = 8.0m,
            PeerScore = 8.0m,
            FinalScore = 8.28m,
            GeneratedAtUtc = DateTimeOffset.UtcNow
        });

        var summaries = new (Employee emp, decimal goalPct, string status, decimal final, int pendingActions, int pendingEval, int submittedEval)[]
        {
            (emps[0], 85m,  "In Review", 82.8m, 2, 5, 12),
            (emps[3], 60m,  "Pending",   0m,    3, 4, 8),
            (emps[4], 100m, "Completed", 88.5m, 0, 0, 10),
            (emps[6], 40m,  "Pending",   0m,    4, 6, 5),
        };
        foreach (var s in summaries)
        {
            db.EmployeeEvaluationSummaries.Add(new EmployeeEvaluationSummary
            {
                EvaluationPeriodId = period.Id,
                EmployeeId = s.emp.Id,
                GoalCompletionPercent = s.goalPct,
                EvaluationStatus = s.status,
                FinalPerScore = s.final,
                PendingActions = s.pendingActions,
                PendingEvaluations = s.pendingEval,
                SubmittedEvaluations = s.submittedEval,
                DaysUntilDeadline = 16,
                EvaluationDeadline = new DateTime(2026, 5, 31),
                ManagerStrengths = "Exceptional technical expertise and leadership in the React migration project. Strong mentorship skills.",
                ManagerDevelopmentAreas = "Focus on improving cross-department communication and strategic planning for larger initiatives.",
                ManagerName = mgr.Name,
                FeedbackUpdatedOn = new DateTime(2026, 5, 13)
            });
        }
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the finalized PER report (and history bands) for the demo employee. Idempotent —
    /// runs on every startup but skips if a report already exists, so it populates an existing
    /// DB after the PerReport migration without a drop/reseed.
    /// </summary>
    public static async Task SeedPerReportAsync(AppDbContext db)
    {
        if (await db.PerReports.AnyAsync()) return;

        var period = await db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Status == PeriodStatus.Active);
        if (period is null) return;

        var aamir = await db.Employees.FirstOrDefaultAsync(e => e.WorkEmail == "employee@pral.com.pk");
        if (aamir is null) return;

        var mgr = await db.Employees.FirstOrDefaultAsync(e => e.Id == aamir.ReportingManagerId)
                  ?? await db.Employees.FirstOrDefaultAsync(e => e.WorkEmail == "manager@pral.com.pk");
        var mgrName = mgr?.Name ?? "Abdul Hafeez Butt";

        // History bands for the report's Promotion & Increment box
        aamir.LastPromotionDate = new DateTime(2024, 1, 1);
        aamir.LastIncrementDate = new DateTime(2025, 7, 1);
        aamir.LastIncrementBand = 3;
        aamir.LastBonusDate = new DateTime(2025, 12, 1);
        aamir.LastBonusBand = 4;

        db.PerReports.Add(new PerReport
        {
            EvaluationPeriodId = period.Id,
            EmployeeId = aamir.Id,
            GoalScorePercent = 80m,
            CompetencyScorePercent = 60m,
            GoalWeightPercent = 70m,
            CompetencyWeightPercent = 30m,
            FinalPercent = 82.6m,
            Band = "Excellent",
            Approved = true,
            ManagerName = mgrName,
            Strengths = "Exceptional technical expertise and code quality\nStrong leadership in mentoring junior developers\nConsistently delivers projects on time",
            DevelopmentAreas = "Enhance cross-department communication\nDevelop strategic planning skills for larger initiatives",
            OverallComments = "Aamir Abdul Aziz has demonstrated outstanding performance throughout Q1 2026. His technical contributions to the React migration project were exceptional, and his dedication to mentoring junior team members has significantly improved team capability. Recommended for promotion consideration.",
            ApprovedBy = mgrName,
            ApprovedOn = new DateTime(2026, 5, 13),
            GoalsSubmittedOn = new DateTime(2026, 3, 15),
            Evaluation360On = new DateTime(2026, 4, 10),
            ManagerReviewOn = new DateTime(2026, 4, 25),
            FinalApprovalOn = new DateTime(2026, 5, 13),
            GoalLines = new List<PerReportGoalLine>
            {
                new() { SortOrder = 1, Title = "React Migration",  WeightPercent = 25, ProgressPercent = 50,  Rating = 3, ContributionPercent = 18.75m },
                new() { SortOrder = 2, Title = "CI/CD Pipeline",   WeightPercent = 20, ProgressPercent = 100, Rating = 4, ContributionPercent = 20m },
                new() { SortOrder = 3, Title = "Team Mentoring",   WeightPercent = 15, ProgressPercent = 70,  Rating = 3, ContributionPercent = 11.3m },
                new() { SortOrder = 4, Title = "Code Quality",     WeightPercent = 30, ProgressPercent = 90,  Rating = 3, ContributionPercent = 22.5m },
                new() { SortOrder = 5, Title = "Documentation",    WeightPercent = 10, ProgressPercent = 80,  Rating = 3, ContributionPercent = 7.5m },
            },
            CompetencyLines = new List<PerReportCompetencyLine>
            {
                new() { SortOrder = 1, Competency = "Problem-Solving & Innovation", Score = 2 },
                new() { SortOrder = 2, Competency = "Teamwork & Collaboration",     Score = 2 },
                new() { SortOrder = 3, Competency = "Integrity",                    Score = 2 },
                new() { SortOrder = 4, Competency = "Ownership",                    Score = 3 },
                new() { SortOrder = 5, Competency = "Leadership",                   Score = 3 },
            }
        });
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds promotion/increment/bonus history for EVERY employee. Idempotent (skips if any
    /// history already exists), so it populates an existing DB after the history migration
    /// without needing a drop/reseed. The demo employee gets the exact Figma values.
    /// </summary>
    public static async Task SeedProfileHistoryAsync(AppDbContext db)
    {
        if (await db.PromotionHistories.AnyAsync()) return;

        var employees = await db.Employees.Include(e => e.Designation).OrderBy(e => e.Id).ToListAsync();

        foreach (var e in employees)
        {
            // Backfill a display title if missing.
            if (string.IsNullOrWhiteSpace(e.JobTitle))
                e.JobTitle = e.Designation?.Name;

            var isDemo = string.Equals(e.WorkEmail, "employee@pral.com.pk", StringComparison.OrdinalIgnoreCase);

            if (isDemo)
            {
                e.JobTitle = "Senior Software Engineer";
                db.PromotionHistories.AddRange(
                    new PromotionHistory { EmployeeId = e.Id, EffectiveDate = new DateTime(2024, 1, 1), FromTitle = "Software Engineer II", ToTitle = "Senior Software Engineer", Note = "Outstanding Performance" },
                    new PromotionHistory { EmployeeId = e.Id, EffectiveDate = new DateTime(2022, 6, 1), FromTitle = "Software Engineer I", ToTitle = "Software Engineer II", Note = "Consistent Growth" },
                    new PromotionHistory { EmployeeId = e.Id, EffectiveDate = new DateTime(2020, 1, 1), FromTitle = null, ToTitle = "Software Engineer I", Note = "New Hire" });
                db.IncrementHistories.AddRange(
                    new IncrementHistory { EmployeeId = e.Id, Year = 2025, Percentage = 12, Amount = 8400 },
                    new IncrementHistory { EmployeeId = e.Id, Year = 2024, Percentage = 15, Amount = 9600 },
                    new IncrementHistory { EmployeeId = e.Id, Year = 2023, Percentage = 10, Amount = 6200 },
                    new IncrementHistory { EmployeeId = e.Id, Year = 2022, Percentage = 8, Amount = 4800 });
                db.BonusHistories.AddRange(
                    new BonusHistory { EmployeeId = e.Id, Year = 2025, BonusType = "Performance Bonus", Quarter = "Q4 2025", Amount = 5000 },
                    new BonusHistory { EmployeeId = e.Id, Year = 2024, BonusType = "Annual Bonus", Quarter = "Q4 2024", Amount = 7500 },
                    new BonusHistory { EmployeeId = e.Id, Year = 2024, BonusType = "Project Completion", Quarter = "Q2 2024", Amount = 3000 },
                    new BonusHistory { EmployeeId = e.Id, Year = 2023, BonusType = "Performance Bonus", Quarter = "Q4 2023", Amount = 4500 });
                continue;
            }

            // Generic, deterministic test data for every other employee.
            var role = e.Designation?.Name ?? "Officer";
            var joinYear = e.RecruitmentDate?.Year ?? 2020;
            var bump = (e.Id % 5) * 300m;

            db.PromotionHistories.AddRange(
                new PromotionHistory { EmployeeId = e.Id, EffectiveDate = new DateTime(joinYear + 3, 3, 1), FromTitle = $"{role} II", ToTitle = $"Senior {role}", Note = "Strong Performance" },
                new PromotionHistory { EmployeeId = e.Id, EffectiveDate = new DateTime(joinYear + 1, 7, 1), FromTitle = $"{role} I", ToTitle = $"{role} II", Note = "Promotion" },
                new PromotionHistory { EmployeeId = e.Id, EffectiveDate = new DateTime(joinYear, 1, 15), FromTitle = null, ToTitle = $"{role} I", Note = "New Hire" });
            db.IncrementHistories.AddRange(
                new IncrementHistory { EmployeeId = e.Id, Year = 2025, Percentage = 10, Amount = 6000 + bump },
                new IncrementHistory { EmployeeId = e.Id, Year = 2024, Percentage = 12, Amount = 5200 + bump },
                new IncrementHistory { EmployeeId = e.Id, Year = 2023, Percentage = 8, Amount = 4200 + bump });
            db.BonusHistories.AddRange(
                new BonusHistory { EmployeeId = e.Id, Year = 2025, BonusType = "Performance Bonus", Quarter = "Q4 2025", Amount = 4000 + bump },
                new BonusHistory { EmployeeId = e.Id, Year = 2024, BonusType = "Annual Bonus", Quarter = "Q4 2024", Amount = 6000 + bump });
        }

        await db.SaveChangesAsync();
    }

    private static Employee New(string hr, string name, Department dept, Designation desig,
        string? wing = null, string? pay = null, string? email = null, string? jobTitle = null) => new()
    {
        HrCode = hr,
        AccountsCode = "ACC-" + new string(hr.Where(char.IsDigit).ToArray()),
        Name = name,
        Title = "Mr",
        DepartmentId = dept.Id,
        DesignationId = desig.Id,
        JobTitle = jobTitle ?? desig.Name,
        Wing = wing,
        PayGroup = pay,
        EmploymentStatus = "CONTRACTUAL",
        WorkEmail = email,
        MobileNumber = "+92 (051) 111-772-572",
        RecruitmentDate = new DateTime(2020, 1, 15),
        IsActive = true
    };

    private static AttributeItem Attr(Competency c, string name, decimal weight, string? description = null)
        => new() { CompetencyId = c.Id, Name = name, Description = description, Weight = weight, IsActive = true };
}
