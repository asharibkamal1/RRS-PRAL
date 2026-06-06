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
            New("HR-2024-0145", "Aamir Abdul Aziz",       dIt, gDev,     wing: "Development Wing", pay: "Grade A - Level 3", email: "employee@pral.com.pk"),
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

        // ---- Competencies & 20 attributes (CRF pre-load) ----
        var integrity = new Competency { Name = "Integrity" };
        var teamwork = new Competency { Name = "Team Work & Collaboration" };
        var problem = new Competency { Name = "Problem Solving & Innovation" };
        var ownership = new Competency { Name = "Takes Ownership" };
        var leadership = new Competency { Name = "Leadership" };
        db.Competencies.AddRange(integrity, teamwork, problem, ownership, leadership);
        await db.SaveChangesAsync();

        var attrs = new List<AttributeItem>
        {
            Attr(integrity, "Compliance", 0.20m), Attr(integrity, "Confidentiality", 0.20m),
            Attr(integrity, "Transparency", 0.20m), Attr(integrity, "Ethics", 0.20m),
            Attr(integrity, "Accountability", 0.20m),
            Attr(teamwork, "Collaboration", 0.20m), Attr(teamwork, "Communication", 0.20m),
            Attr(teamwork, "Support", 0.20m), Attr(teamwork, "Alignment", 0.20m),
            Attr(teamwork, "Teamwork", 0.20m),
            Attr(problem, "Analysis", 0.25m), Attr(problem, "Problem-Solving", 0.25m),
            Attr(problem, "Innovation", 0.25m), Attr(problem, "Improvement", 0.25m),
            Attr(ownership, "Accountability", 0.33m), Attr(ownership, "Commitment", 0.33m),
            Attr(ownership, "Responsibility", 0.34m),
            Attr(leadership, "Motivation", 0.33m), Attr(leadership, "Decision-Making", 0.33m),
            Attr(leadership, "Vision", 0.34m),
        };
        db.Attributes.AddRange(attrs);
        await db.SaveChangesAsync();

        // ---- Designation→Attribute default map: the 10 Integrity + Team Work attributes per designation ----
        var defaultTen = attrs.Where(a => a.CompetencyId == integrity.Id || a.CompetencyId == teamwork.Id).ToList();
        foreach (var g in new[] { gManager, gDev, gDba, gHr, gQa })
            foreach (var a in defaultTen)
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

        // ---- Sample peer ratings for Aamir from manager (all 10 mapped attributes) ----
        var ratingPeriod = await db.RatingPeriods.FirstAsync(rp => rp.EvaluationPeriodId == period.Id);
        foreach (var a in defaultTen)
        {
            db.CompetencyRatings.Add(new CompetencyRating
            {
                RatingPeriodId = ratingPeriod.Id,
                RateeEmployeeId = aamir.Id,
                RatorEmployeeId = mgr.Id,
                AttributeId = a.Id,
                Rating = 8,
                Status = RatingStatus.Done
            });
        }
        await db.SaveChangesAsync();
    }

    private static Employee New(string hr, string name, Department dept, Designation desig,
        string? wing = null, string? pay = null, string? email = null) => new()
    {
        HrCode = hr,
        AccountsCode = "ACC-" + new string(hr.Where(char.IsDigit).ToArray()),
        Name = name,
        Title = "Mr",
        DepartmentId = dept.Id,
        DesignationId = desig.Id,
        Wing = wing,
        PayGroup = pay,
        EmploymentStatus = "CONTRACTUAL",
        WorkEmail = email,
        RecruitmentDate = new DateTime(2020, 1, 15),
        IsActive = true
    };

    private static AttributeItem Attr(Competency c, string name, decimal weight)
        => new() { CompetencyId = c.Id, Name = name, Weight = weight, IsActive = true };
}
