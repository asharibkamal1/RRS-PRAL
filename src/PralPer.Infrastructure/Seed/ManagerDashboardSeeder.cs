using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>
/// Seeds dummy manager-dashboard snapshot data (KPIs, team performance, pending approvals,
/// raters, peer-evaluation snapshots + 360 lines). Idempotent — skips if already seeded.
/// </summary>
public static class ManagerDashboardSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.ManagerRaters.AnyAsync()) return;

        db.ManagerDashboardStats.Add(new ManagerDashboardStat
        {
            TeamEvaluationProgress = 78,
            PendingApprovals = 12,
            AverageTeamScore = 82.5m,
            EmployeesAwaitingReview = 8,
            TotalEvaluationsAssigned = 25,
            PendingEvaluations = 12,
            SubmittedEvaluations = 13,
            DaysUntilDeadline = 16
        });

        db.DeptPerformances.AddRange(
            new() { SortOrder = 1, Department = "IT", Completed = 140, Pending = 12 },
            new() { SortOrder = 2, Department = "HR", Completed = 88, Pending = 6 },
            new() { SortOrder = 3, Department = "Finance", Completed = 230, Pending = 15 },
            new() { SortOrder = 4, Department = "Operations", Completed = 315, Pending = 30 },
            new() { SortOrder = 5, Department = "Sales", Completed = 190, Pending = 12 },
            new() { SortOrder = 6, Department = "Marketing", Completed = 180, Pending = 10 });

        db.ManagerApprovals.AddRange(
            new() { SortOrder = 1, EmployeeName = "Aamir Abdul Aziz", ApprovalType = "Goal Submission Review", AgoText = "2 hours ago" },
            new() { SortOrder = 2, EmployeeName = "Abdul Rehman", ApprovalType = "PER Final Approval", AgoText = "5 hours ago" },
            new() { SortOrder = 3, EmployeeName = "Abbas Ali", ApprovalType = "Competency Review", AgoText = "1 day ago" },
            new() { SortOrder = 4, EmployeeName = "Abdul Moeed Ahmad", ApprovalType = "Goal Weight Approval", AgoText = "1 day ago" });

        await db.SaveChangesAsync();

        // ---- Raters ----
        var jabbar = new ManagerRater { SortOrder = 1, Name = "Abdul Jabbar", IsActiveRator = true };
        var moeed = new ManagerRater { SortOrder = 2, Name = "Abdul Moeed Ahmad", IsActiveRator = true };
        var mohsin = new ManagerRater { SortOrder = 3, Name = "Abdul Mohsin", IsActiveRator = false };
        var moqtadir = new ManagerRater { SortOrder = 4, Name = "Abdul Moqtadir", IsActiveRator = true };
        var qudoos = new ManagerRater { SortOrder = 5, Name = "Abdul Qudoos Sheikh", IsActiveRator = false };
        var rauf = new ManagerRater { SortOrder = 6, Name = "Abdul Rauf Arshad", IsActiveRator = true };
        var razzaq = new ManagerRater { SortOrder = 7, Name = "Abdul Razzaq", IsActiveRator = true };
        var samad = new ManagerRater { SortOrder = 8, Name = "Abdul Samad", IsActiveRator = true };
        db.ManagerRaters.AddRange(jabbar, moeed, mohsin, moqtadir, qudoos, rauf, razzaq, samad);
        await db.SaveChangesAsync();

        // ---- Abdul Jabbar's ratees (match popup), with full 360 lines for Muhammad Ashfaq ----
        var ashfaq = new PeerEvaluationSnapshot
        {
            RaterId = jabbar.Id, SortOrder = 1, RateeName = "Muhammad Ashfaq", RateeDepartment = "Admin",
            RateeJobTitle = "Dt. Manager Admin", Status = "Completed", PeriodName = "Q1- 2026",
            AverageRating = 6.0m, CompetenciesRated = 12, AssessmentScorePercent = 60.0m,
            Lines = new List<PeerEvaluationLine>
            {
                new() { SortOrder = 1, Competency = "Leadership", AttributeName = "Leadership", Remarks = "Has ability to lead teams and inspire others", Score = 4 },
                new() { SortOrder = 2, Competency = "Leadership", AttributeName = "Mentoring", Remarks = "Helps employees learn new skills", Score = 2 },
                new() { SortOrder = 3, Competency = "Leadership", AttributeName = "Strategic Thinking", Remarks = "Analytical thinking and solution finding", Score = 3 },
                new() { SortOrder = 4, Competency = "Takes Ownership", AttributeName = "Proactive", Remarks = "Prepares for projects in advance and does due diligence", Score = 3 },
                new() { SortOrder = 5, Competency = "Takes Ownership", AttributeName = "Quality Focus", Remarks = "Attention to detail and quality standards", Score = 3 },
                new() { SortOrder = 6, Competency = "Teamwork & Collaboration", AttributeName = "Communication", Remarks = "Clear and effective communication", Score = 2 },
                new() { SortOrder = 7, Competency = "Teamwork & Collaboration", AttributeName = "Collaboration", Remarks = "Communicates tasks clearly and gives timely feedback", Score = 2 },
                new() { SortOrder = 8, Competency = "Integrity", AttributeName = "Ethics", Remarks = "Adheres to moral and ethical principles", Score = 2 },
                new() { SortOrder = 9, Competency = "Integrity", AttributeName = "Accountability", Remarks = "Owns mistakes and accepts responsibility", Score = 2 },
                new() { SortOrder = 10, Competency = "Problem Solving & Innovation", AttributeName = "Creativity", Remarks = "Generates new implementable ideas", Score = 1 },
                new() { SortOrder = 11, Competency = "Problem Solving & Innovation", AttributeName = "Analytical Thinking", Remarks = "Able to break down complex problems and use evidence to decide", Score = 3 },
                new() { SortOrder = 12, Competency = "Problem Solving & Innovation", AttributeName = "Curiosity", Remarks = "Willing to ask questions and consider alternative perspectives", Score = 2 },
            }
        };
        db.PeerEvaluationSnapshots.Add(ashfaq);
        AddRatees(db, jabbar, ("Khurram Khan", "HR", "Completed"), ("Abdul Moqtadir", "DB", "Pending"),
            ("Kashif Khan", "QC", "Pending"), ("Muhammad Salman", "QC", "Completed"));

        // ---- Other raters: ratee rows to drive the table counts (no detail lines needed) ----
        AddRatees(db, moeed, ("Imran Ali", "IT", "Completed"), ("Zeeshan Tariq", "HR", "Completed"), ("Naveed Akhtar", "Finance", "Pending"));
        AddRatees(db, moqtadir, ("Bilal Ahmed", "IT", "Completed"), ("Hamza Sheikh", "DB", "Completed"), ("Usman Ali", "QC", "Pending"), ("Saad Khan", "Sales", "Pending"));
        AddRatees(db, rauf, ("Faisal Iqbal", "IT", "Completed"), ("Asad Mahmood", "Operations", "Completed"), ("Tariq Jameel", "Finance", "Completed"), ("Noman Shah", "HR", "Pending"));
        AddRatees(db, razzaq, ("Adeel Raza", "IT", "Completed"), ("Kamran Ali", "Sales", "Completed"), ("Waqas Ahmed", "Marketing", "Completed"), ("Salman Tariq", "QC", "Completed"), ("Rizwan Khan", "DB", "Pending"), ("Junaid Akbar", "Operations", "Pending"));
        AddRatees(db, samad, ("Shahid Afridi", "IT", "Completed"), ("Yasir Shah", "Finance", "Completed"), ("Danish Ali", "HR", "Completed"), ("Owais Khan", "QC", "Pending"), ("Hassan Raza", "Sales", "Pending"));

        await db.SaveChangesAsync();
    }

    private static void AddRatees(AppDbContext db, ManagerRater rater, params (string name, string dept, string status)[] ratees)
    {
        var i = 2;
        foreach (var (name, dept, status) in ratees)
        {
            db.PeerEvaluationSnapshots.Add(new PeerEvaluationSnapshot
            {
                RaterId = rater.Id, SortOrder = i++, RateeName = name, RateeDepartment = dept,
                RateeJobTitle = "Officer", Status = status, PeriodName = "Q1- 2026",
                AverageRating = 6.0m, CompetenciesRated = 12, AssessmentScorePercent = 60.0m
            });
        }
    }
}
