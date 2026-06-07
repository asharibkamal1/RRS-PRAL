using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>Seeds the admin dashboard snapshot (KPIs, charts, recent activity) with the Figma figures. Idempotent.</summary>
public static class AdminDashboardSeeder
{
    // Department-wise progress (Completed, Pending)
    private static readonly (string Dept, int Completed, int Pending)[] Dept =
    {
        ("HR", 140, 10), ("QC", 85, 8), ("UI/UX", 230, 15), ("Dev Ops", 320, 20),
        ("Finance", 185, 12), ("Admin", 175, 10), ("DB", 180, 14),
    };

    // Evaluation timeline (Jan–May)
    private static readonly (string Month, int Value)[] Timeline =
    {
        ("Jan", 145), ("Feb", 195), ("Mar", 235), ("Apr", 290), ("May", 320),
    };

    // Performance rating distribution (score 0–4) — bell curve
    private static readonly (int Score, string Label, decimal Percent)[] Rating =
    {
        (0, "Below Expectations", 5), (1, "Needs Improvement", 15), (2, "Meets Expectations", 60),
        (3, "Excellent", 15), (4, "Outstanding", 5),
    };

    private static readonly (string Actor, string Description, string Ago)[] Activity =
    {
        ("Aamir Abdul Aziz",      "Completed PER submission",        "2 minutes ago"),
        ("Abdul Mohsin",          "Updated goal weights",            "15 minutes ago"),
        ("Abbas Ali",             "Submitted 360° evaluation",       "1 hour ago"),
        ("Abdul Moeed Ahmad",     "Assigned as rator",               "2 hours ago"),
        ("Abdul Wadood Sherani",  "Created new attribute: Leadership","3 hours ago"),
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.AdminDashboardStats.AnyAsync()) return;

        db.AdminDashboardStats.Add(new AdminDashboardStat
        {
            TotalEmployees = 1234,
            EmployeesTrend = 12m,
            EvaluationsPending = 87,
            PendingTrend = -5m,
            CompletedReviews = 1147,
            CompletedTrend = 18m,
            ActiveCycleName = "Q1 2026",
            ActiveCyclePercent = 92m,
            StatusCompletedPercent = 93m,
            StatusNotStartedPercent = 2m,
            StatusInProgressPercent = 5m
        });

        var order = 0;
        foreach (var (dept, completed, pending) in Dept)
            db.AdminDashboardSeriesPoints.Add(new AdminDashboardSeriesPoint
            { Category = "Dept", SortOrder = order++, Label = dept, ValueA = completed, ValueB = pending });

        order = 0;
        foreach (var (month, value) in Timeline)
            db.AdminDashboardSeriesPoints.Add(new AdminDashboardSeriesPoint
            { Category = "Timeline", SortOrder = order++, Label = month, ValueA = value });

        foreach (var (score, label, percent) in Rating)
            db.AdminDashboardSeriesPoints.Add(new AdminDashboardSeriesPoint
            { Category = "Rating", SortOrder = score, Label = label, ValueA = percent });

        order = 0;
        foreach (var (actor, desc, ago) in Activity)
            db.AdminActivities.Add(new AdminActivity
            { SortOrder = order++, ActorName = actor, Description = desc, AgoText = ago });

        await db.SaveChangesAsync();
    }
}
