using PralPer.Application.Catalog;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Services;
using Xunit;

namespace PralPer.UnitTests;

public class CatalogAdminServiceTests
{
    private static Competency SeedCompetency(TestDb t, string name = "Teamwork & Collaboration")
    {
        var c = new Competency { Name = name };
        t.Db.Competencies.Add(c);
        t.Db.SaveChanges();
        return c;
    }

    [Fact]
    public async Task CreateAttribute_StoresFraction_AndReadsBackAsPercent()
    {
        using var t = new TestDb();
        var comp = SeedCompetency(t);
        var svc = new CatalogAdminService(t.Db);

        var res = await svc.CreateAttributeAsync(new AttributeInput
        {
            CompetencyId = comp.Id, Name = "Communication", Weight = 25m, IsActive = true
        });
        Assert.True(res.Succeeded);

        // Stored as a fraction (fits decimal(5,4))...
        var stored = Assert.Single(t.Db.Attributes.ToList());
        Assert.Equal(0.25m, stored.Weight);

        // ...surfaced to the UI as a whole percent.
        var rows = await svc.GetAttributesAsync(null, null);
        Assert.Equal(25m, Assert.Single(rows).Weight);
    }

    [Fact]
    public async Task CreateAttribute_RejectsWeightOutOfRange()
    {
        using var t = new TestDb();
        var comp = SeedCompetency(t);
        var svc = new CatalogAdminService(t.Db);

        var res = await svc.CreateAttributeAsync(new AttributeInput { CompetencyId = comp.Id, Name = "X", Weight = 150m });

        Assert.False(res.Succeeded);
        Assert.Empty(t.Db.Attributes.ToList());
    }

    [Fact]
    public async Task CreateAttribute_RejectsDuplicateInSameCompetency()
    {
        using var t = new TestDb();
        var comp = SeedCompetency(t);
        var svc = new CatalogAdminService(t.Db);

        await svc.CreateAttributeAsync(new AttributeInput { CompetencyId = comp.Id, Name = "Empathy", Weight = 10m });
        var res = await svc.CreateAttributeAsync(new AttributeInput { CompetencyId = comp.Id, Name = "Empathy", Weight = 20m });

        Assert.False(res.Succeeded);
        Assert.Single(t.Db.Attributes.ToList());
    }

    [Fact]
    public async Task CreateLevelMap_CreatesAttributeIfMissing_AndSummarizesPercent()
    {
        using var t = new TestDb();
        var comp = SeedCompetency(t);
        var level = t.SeedDesignation("Staff");   // job level lives in Designations
        var svc = new CatalogAdminService(t.Db);

        var res = await svc.CreateLevelMapAsync(comp.Id, "Time Management", level.Id, 30m);
        Assert.True(res.Succeeded);

        var maps = await svc.GetLevelMapsAsync();
        var map = Assert.Single(maps);
        Assert.Equal("Time Management", map.Attribute);
        Assert.Equal("Staff", map.Level);
        Assert.Equal(30m, map.Weight);

        var summary = Assert.Single(await svc.GetLevelWeightSummaryAsync());
        Assert.Equal(30m, summary.TotalWeight);
    }

    [Fact]
    public async Task UpdateLevelMapWeight_PersistsNewPercent()
    {
        using var t = new TestDb();
        var comp = SeedCompetency(t);
        var level = t.SeedDesignation("Staff");
        var svc = new CatalogAdminService(t.Db);
        await svc.CreateLevelMapAsync(comp.Id, "Adaptability", level.Id, 10m);
        var map = Assert.Single(await svc.GetLevelMapsAsync());

        var res = await svc.UpdateLevelMapWeightAsync(map.Id, 45m);

        Assert.True(res.Succeeded);
        Assert.Equal(45m, (await svc.GetLevelMapsAsync()).Single().Weight);
    }
}
