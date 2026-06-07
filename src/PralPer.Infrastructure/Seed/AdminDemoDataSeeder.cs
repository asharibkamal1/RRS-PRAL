using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>
/// Populates the real operational tables with a broad demo dataset (departments, managers + teams,
/// PER reports across score bands and months, goals, rater assignments and 360° ratings) so the
/// live Administrator Dashboard renders full, meaningful aggregates. Idempotent.
/// </summary>
public static class AdminDemoDataSeeder
{
    private static readonly (string Dept, int Team)[] Departments =
    {
        ("HR", 5), ("QC", 4), ("UI/UX", 6), ("Dev Ops", 7), ("Finance", 5), ("Admin", 5), ("DB", 6),
    };

    private static readonly string[] Managers =
    {
        "Abdul Jabbar", "Abdul Moeed Ahmad", "Abdul Mohsin", "Abdul Moqtadir",
        "Abdul Qudoos Sheikh", "Abdul Rauf Arshad", "Abdul Razzaq",
    };

    private static readonly string[] FirstNames =
    {
        "Ahmed", "Bilal", "Usman", "Hamza", "Saad", "Zain", "Imran", "Kashif", "Salman", "Adeel",
        "Faisal", "Nabeel", "Tariq", "Waqas", "Yasir", "Asad", "Danish", "Faraz", "Hassan", "Junaid",
    };
    private static readonly string[] LastNames =
    {
        "Khan", "Ahmed", "Malik", "Sheikh", "Butt", "Raza", "Iqbal", "Aslam", "Hussain", "Javed",
    };

    // FinalPercent pattern producing a bell-shaped distribution (5/15/60/15/5 across buckets 0-4).
    private static readonly decimal[] FinalPattern = BuildBell();

    private static decimal[] BuildBell()
    {
        var list = new List<decimal>();
        list.AddRange(Enumerable.Repeat(35m, 1));   // bucket 0
        list.AddRange(Enumerable.Repeat(52m, 3));   // bucket 1
        list.AddRange(Enumerable.Repeat(72m, 12));  // bucket 2
        list.AddRange(Enumerable.Repeat(85m, 3));   // bucket 3
        list.AddRange(Enumerable.Repeat(95m, 1));   // bucket 4
        return list.ToArray();
    }

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Departments.AnyAsync(d => d.Name == "Dev Ops")) return;

        var periodId = await db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync();
        if (periodId == 0) return;

        var ratingPeriodId = await db.RatingPeriods
            .Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync();
        var firstAttributeId = await db.Attributes.Select(a => a.Id).FirstOrDefaultAsync();

        var managerDesigId = await db.Designations.Where(d => d.Name.Contains("Manager")).Select(d => d.Id).FirstOrDefaultAsync();
        var staffDesigId = await db.Designations.Where(d => !d.Name.Contains("Manager")).Select(d => d.Id).FirstOrDefaultAsync();
        if (managerDesigId == 0) managerDesigId = await db.Designations.Select(d => d.Id).FirstAsync();
        if (staffDesigId == 0) staffDesigId = managerDesigId;

        // 1) Departments
        foreach (var (dept, _) in Departments)
            db.Departments.Add(new Department { Name = dept });
        await db.SaveChangesAsync();

        var deptIds = new Dictionary<string, int>();
        foreach (var (dept, _) in Departments)
            deptIds[dept] = await db.Departments.Where(x => x.Name == dept).Select(x => x.Id).FirstAsync();

        // 2) Managers (one per department)
        var managers = new List<Employee>();
        for (var di = 0; di < Departments.Length; di++)
        {
            managers.Add(new Employee
            {
                HrCode = $"DEMO-M{di + 1:00}",
                AccountsCode = $"ACC-M{di + 1:00}",
                Name = Managers[di],
                Title = "Mr",
                DepartmentId = deptIds[Departments[di].Dept],
                DesignationId = managerDesigId,
                JobTitle = "Manager",
                EmploymentStatus = "PERMANENT",
                MobileNumber = "+92 (051) 111-772-572",
                RecruitmentDate = new DateTime(2018, 6, 1),
                IsActive = true
            });
        }
        db.Employees.AddRange(managers);
        await db.SaveChangesAsync();

        // 3) Reports per manager
        var reports = new List<Employee>();
        var n = 0;
        for (var di = 0; di < Departments.Length; di++)
        {
            var (dept, team) = Departments[di];
            for (var k = 0; k < team; k++)
            {
                var name = $"{FirstNames[n % FirstNames.Length]} {LastNames[(n / FirstNames.Length + k) % LastNames.Length]}";
                reports.Add(new Employee
                {
                    HrCode = $"DEMO-E{di + 1:00}{k + 1:00}",
                    AccountsCode = $"ACC-E{di + 1:00}{k + 1:00}",
                    Name = name,
                    Title = "Mr",
                    DepartmentId = deptIds[dept],
                    DesignationId = staffDesigId,
                    JobTitle = "Officer",
                    EmploymentStatus = "CONTRACTUAL",
                    MobileNumber = "+92 (051) 111-772-572",
                    RecruitmentDate = new DateTime(2021, 1, 15),
                    ReportingManagerId = managers[di].Id,
                    IsActive = true
                });
                n++;
            }
        }
        db.Employees.AddRange(reports);
        await db.SaveChangesAsync();

        // 4) PER reports (≈80% of reports), spread over Jan–May, FinalPercent bell distribution
        var goalsForInProgress = new List<Employee>();
        for (var i = 0; i < reports.Count; i++)
        {
            var e = reports[i];
            if (i % 5 == 4) { goalsForInProgress.Add(e); continue; } // leave ~20% without a report (in progress)

            var final = FinalPattern[i % FinalPattern.Length];
            var (band, _) = BandOf(final);
            var approved = final >= 40m;
            var month = 1 + (i % 5); // Jan..May
            var approvedOn = approved ? new DateTime(2026, month, 10) : (DateTime?)null;

            db.PerReports.Add(new PerReport
            {
                EvaluationPeriodId = periodId,
                EmployeeId = e.Id,
                GoalScorePercent = Math.Min(100m, final + 4m),
                CompetencyScorePercent = Math.Max(0m, final - 6m),
                GoalWeightPercent = 70m,
                CompetencyWeightPercent = 30m,
                FinalPercent = final,
                Band = band,
                Approved = approved,
                ManagerName = managers[i % managers.Count].Name,
                ApprovedOn = approvedOn,
                GoalsSubmittedOn = new DateTime(2026, 1, 15),
                Evaluation360On = new DateTime(2026, 2, 10),
                ManagerReviewOn = new DateTime(2026, month, 5),
                FinalApprovalOn = approvedOn
            });
        }

        // 5) Goals for the in-progress group (so Status Distribution shows In Progress)
        foreach (var e in goalsForInProgress)
            for (var g = 1; g <= 3; g++)
                db.Goals.Add(new Goal
                {
                    EvaluationPeriodId = periodId, EmployeeId = e.Id, GoalNo = g,
                    Title = $"Objective {g}", ProgressPercent = 40 + g * 10, WeightPercent = g == 3 ? 40 : 30, Rating = 0
                });

        // 6) Rater assignments (recent activity)
        for (var di = 0; di < Departments.Length; di++)
        {
            var teamReports = reports.Where(r => r.DepartmentId == deptIds[Departments[di].Dept]).Take(2).ToList();
            foreach (var ratee in teamReports)
                db.RatorAssignments.Add(new RatorAssignment
                {
                    EvaluationPeriodId = periodId, RateeEmployeeId = ratee.Id, RatorEmployeeId = managers[di].Id
                });
        }

        // 7) A few completed 360° ratings (recent activity)
        if (ratingPeriodId != 0 && firstAttributeId != 0)
        {
            for (var i = 0; i < Math.Min(6, reports.Count); i++)
                db.CompetencyRatings.Add(new CompetencyRating
                {
                    RatingPeriodId = ratingPeriodId,
                    RateeEmployeeId = reports[i].Id,
                    RatorEmployeeId = managers[i % managers.Count].Id,
                    AttributeId = firstAttributeId,
                    Rating = 7,
                    Status = RatingStatus.Done
                });
        }

        await db.SaveChangesAsync();
    }

    private static (string Band, bool Approved) BandOf(decimal final) => final switch
    {
        >= 80 => ("Excellent", true),
        >= 70 => ("Very Good", true),
        >= 60 => ("Good", true),
        >= 40 => ("Needs Improvement", true),
        _ => ("Below Expectations", false)
    };
}
