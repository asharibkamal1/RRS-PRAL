using Microsoft.EntityFrameworkCore;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Persistence;

namespace PralPer.Infrastructure.Seed;

/// <summary>
/// Seeds the competency catalog used by the Attribute Administration and Level Mapping screens:
/// the 5 competencies, their attributes, the job levels (stored as Designation rows), and the
/// attribute→level weight mappings. Fully idempotent (get-or-create), so it also populates an
/// existing database without a drop/reseed.
/// </summary>
public static class CompetencyCatalogSeeder
{
    /// <summary>Job levels shown in the "Level" dropdown (seeded as Designation rows).</summary>
    public static readonly string[] JobLevels =
        { "All Levels", "Entry Level", "Staff", "Associates", "Professionals", "Management", "Special" };

    public static readonly string[] Competencies =
        { "Teamwork & Collaboration", "Takes Ownership", "Problem solving & Innovation", "Leadership", "Integrity" };

    // Competency, Attribute, Description, Weight %, Active
    private static readonly (string Comp, string Name, string Desc, decimal Weight, bool Active)[] AttributeDefs =
    {
        ("Teamwork & Collaboration", "Communication Skills",     "Ability to convey information clearly and effectively across teams", 25, true),
        ("Teamwork & Collaboration", "Empathy",                  "Understanding the perspectives and feelings of team members", 15, true),
        ("Teamwork & Collaboration", "Adaptability",             "Flexibility in responding to changing business needs", 15, true),
        ("Teamwork & Collaboration", "Conflict Resolution",      "Addressing disagreements to reach mutually acceptable solutions", 15, true),
        ("Teamwork & Collaboration", "Work Ethic",               "Dedication and commitment to delivering quality work consistently", 15, true),
        ("Teamwork & Collaboration", "Patience",                 "Ability to remain calm and composed under pressure", 15, true),
        ("Teamwork & Collaboration", "Teamwork & Collaboration", "Working effectively with others toward shared goals", 20, true),
        ("Teamwork & Collaboration", "Time Management",          "Efficient use of time and prioritization of tasks", 15, true),

        ("Takes Ownership", "Initiative",       "Self-directed energy to assess situations and take action independently", 20, true),
        ("Takes Ownership", "Proactive",        "Anticipating future events or problems and taking action", 25, true),
        ("Takes Ownership", "Takes initiative", "Steps up to take responsibility without being asked", 15, true),
        ("Takes Ownership", "Self motivated",   "Driven to achieve goals without external pressure", 15, true),
        ("Takes Ownership", "Result Oriented",  "Focused on achieving measurable outcomes", 15, true),

        ("Problem solving & Innovation", "Knowledge management",        "Capturing and sharing organizational knowledge effectively", 30, true),
        ("Problem solving & Innovation", "Creativity & Experimentation","Exploring new ideas through experimentation", 25, true),
        ("Problem solving & Innovation", "AI Integration",             "Leveraging AI tools to improve workflows", 10, true),
        ("Problem solving & Innovation", "AI driven troubleshooting",  "Using AI to diagnose and resolve issues", 15, true),
        ("Problem solving & Innovation", "Research",                   "Investigating problems to find evidence-based solutions", 10, true),
        ("Problem solving & Innovation", "Innovation & Creativity",    "Generating novel ideas and creative solutions", 20, true),

        ("Leadership", "Strategic Thinking",   "Planning long-term direction aligned with organizational goals", 20, true),
        ("Leadership", "Leadership & Vision",  "Setting direction and inspiring others toward a vision", 25, true),
        ("Leadership", "Mentoring & Coaching", "Developing others through guidance and feedback", 15, true),
        ("Leadership", "Decision Making",      "Making sound, timely decisions", 20, true),
        ("Leadership", "Delegation",           "Assigning responsibility effectively across the team", 15, true),

        ("Integrity", "Ethical Conduct",                "Acting in line with ethical principles", 15, true),
        ("Integrity", "Accountability",                 "Taking responsibility for actions and outcomes", 15, true),
        ("Integrity", "Professional Conduct",           "Maintaining professionalism in all interactions", 15, true),
        ("Integrity", "Information Security Compliance", "Adhering to information security policies", 20, true),
        ("Integrity", "Data Security & Privacy",        "Protecting sensitive data and respecting privacy", 25, true),
        ("Integrity", "Confidentiality",                "Safeguarding confidential information", 30, true),
        ("Integrity", "Honesty",                        "Truthful and transparent in all dealings", 15, true),
    };

    // Competency, Attribute, Level, Weight %
    private static readonly (string Comp, string Attr, string Level, decimal Weight)[] LevelMapDefs =
    {
        ("Teamwork & Collaboration", "Communication Skills",      "All Levels",    25),
        ("Teamwork & Collaboration", "Teamwork & Collaboration",  "All Levels",    20),
        ("Teamwork & Collaboration", "Adaptability",              "Entry Level",   15),
        ("Teamwork & Collaboration", "Time Management",           "Staff",         15),
        ("Teamwork & Collaboration", "Work Ethic",                "All Levels",    15),
        ("Takes Ownership",          "Proactive",                 "Associates",    25),
        ("Takes Ownership",          "Takes initiative",          "Professionals", 15),
        ("Takes Ownership",          "Self motivated",            "Entry Level",   15),
        ("Integrity",                "Ethical Conduct",           "All Levels",    15),
        ("Integrity",                "Accountability",            "All Levels",    15),
        ("Integrity",                "Professional Conduct",      "Staff",         15),
        ("Leadership",               "Strategic Thinking",        "Management",    20),
        ("Leadership",               "Leadership & Vision",       "Special",       25),
        ("Leadership",               "Mentoring & Coaching",      "Management",    15),
        ("Leadership",               "Decision Making",           "Management",    20),
        ("Problem solving & Innovation", "Knowledge management",        "Professionals", 30),
        ("Problem solving & Innovation", "Creativity & Experimentation","Professionals", 25),
        ("Problem solving & Innovation", "AI Integration",             "Professionals", 10),
        ("Problem solving & Innovation", "AI driven troubleshooting",  "Associates",    15),
        ("Problem solving & Innovation", "Research",                   "Associates",    10),
        ("Problem solving & Innovation", "Innovation & Creativity",    "Professionals", 20),
        ("Integrity",                "Information Security Compliance", "Associates",    20),
        ("Integrity",                "Data Security & Privacy",        "Associates",    25),
        ("Integrity",                "Confidentiality",                "Special",       30),
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        // 1) Competencies
        var comps = new Dictionary<string, int>();
        foreach (var name in Competencies)
        {
            var c = await db.Competencies.FirstOrDefaultAsync(x => x.Name == name);
            if (c is null) { c = new Competency { Name = name }; db.Competencies.Add(c); }
            comps[name] = c.Id;
        }
        await db.SaveChangesAsync();
        foreach (var name in Competencies)
            comps[name] = (await db.Competencies.FirstAsync(x => x.Name == name)).Id;

        // 2) Job levels (as Designation rows)
        foreach (var lvl in JobLevels)
            if (!await db.Designations.AnyAsync(d => d.Name == lvl))
                db.Designations.Add(new Designation { Name = lvl });
        await db.SaveChangesAsync();

        // 3) Attributes
        foreach (var (comp, attrName, desc, weight, active) in AttributeDefs)
        {
            var compId = comps[comp];
            if (!await db.Attributes.AnyAsync(a => a.CompetencyId == compId && a.Name == attrName))
                db.Attributes.Add(new AttributeItem
                {
                    CompetencyId = compId, Name = attrName, Description = desc, Weight = weight / 100m, IsActive = active
                });
        }
        await db.SaveChangesAsync();

        // 4) Attribute → level weight maps
        var levelIds = await db.Designations.Where(d => JobLevels.Contains(d.Name))
            .ToDictionaryAsync(d => d.Name, d => d.Id);

        foreach (var (comp, attrName, level, weight) in LevelMapDefs)
        {
            var compId = comps[comp];
            var attrId = await db.Attributes
                .Where(a => a.CompetencyId == compId && a.Name == attrName).Select(a => a.Id).FirstOrDefaultAsync();
            if (attrId == 0 || !levelIds.TryGetValue(level, out var levelId)) continue;

            if (!await db.DesignationAttributeMaps.AnyAsync(m => m.DesignationId == levelId && m.AttributeId == attrId))
                db.DesignationAttributeMaps.Add(new DesignationAttributeMap
                {
                    DesignationId = levelId, AttributeId = attrId, Weight = weight / 100m
                });
        }
        await db.SaveChangesAsync();
    }
}
