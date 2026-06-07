using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Domain.Enums;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>
/// Seeds the manager's direct reports (Figma team members) and the goals they submitted for the
/// active period, so the Goal Assessment screen has data to assess. Idempotent.
/// </summary>
public static class ManagerTeamSeeder
{
    private static readonly (string Hr, string Name, string Email)[] Members =
    {
        ("PRAL-TEAM-101", "Imran Ahmed",     "imran.ahmed@pral.com.pk"),
        ("PRAL-TEAM-102", "Usman Tariq",     "usman.tariq@pral.com.pk"),
        ("PRAL-TEAM-103", "Aslam Khan",      "aslam.khan@pral.com.pk"),
        ("PRAL-TEAM-104", "Muhammad Kashif", "muhammad.kashif@pral.com.pk"),
        ("PRAL-TEAM-105", "Salman Ahmed",    "salman.ahmed@pral.com.pk"),
    };

    // Goals each team member submitted (Rating 0 = not yet assessed by the manager). Weights total 100.
    private static readonly (string Title, decimal Progress, decimal Weight)[] GoalTemplate =
    {
        ("Complete React Migration Project", 50, 30),
        ("Implement CI/CD Pipeline",         100, 15),
        ("Mentor Junior Developers",         90, 15),
        ("Code Quality Improvements",        70, 30),
        ("Documentation Updates",            80, 10),
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Employees.AnyAsync(e => e.HrCode == "PRAL-TEAM-101")) return;

        var manager = await db.Employees.FirstOrDefaultAsync(e => e.WorkEmail == "manager@pral.com.pk");
        if (manager is null) return;

        var designationId = await db.Designations
            .Where(d => d.Name.Contains("Engineer") || d.Name.Contains("Developer"))
            .Select(d => d.Id).FirstOrDefaultAsync();
        if (designationId == 0) designationId = manager.DesignationId;

        var periodId = await db.EvaluationPeriods
            .Where(p => p.Status == PeriodStatus.Active).Select(p => p.Id).FirstOrDefaultAsync();

        foreach (var (hr, name, email) in Members)
        {
            db.Employees.Add(new Employee
            {
                HrCode = hr,
                AccountsCode = "ACC-" + new string(hr.Where(char.IsDigit).ToArray()),
                Name = name,
                Title = "Mr",
                DepartmentId = manager.DepartmentId,
                DesignationId = designationId,
                JobTitle = "Software Engineer",
                Wing = "Development Wing",
                PayGroup = "Grade A - Level 2",
                EmploymentStatus = "CONTRACTUAL",
                WorkEmail = email,
                MobileNumber = "+92 (051) 111-772-572",
                RecruitmentDate = new DateTime(2021, 3, 1),
                ReportingManagerId = manager.Id,
                IsActive = true
            });
        }
        await db.SaveChangesAsync();

        if (periodId == 0) return;   // no active period -> employees seeded, goals skipped

        var teamIds = await db.Employees
            .Where(e => Members.Select(m => m.Hr).Contains(e.HrCode))
            .Select(e => e.Id).ToListAsync();

        foreach (var empId in teamIds)
        {
            var no = 1;
            foreach (var (title, progress, weight) in GoalTemplate)
            {
                db.Goals.Add(new Goal
                {
                    EvaluationPeriodId = periodId,
                    EmployeeId = empId,
                    GoalNo = no++,
                    Title = title,
                    Description = null,
                    ProgressPercent = progress,
                    WeightPercent = weight,
                    Rating = 0
                });
            }
        }
        await db.SaveChangesAsync();
    }
}
