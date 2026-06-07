using Microsoft.EntityFrameworkCore;
using PralPer.Application.Catalog;
using PralPer.Application.Common;
using PralPer.Application.Services;
using PralPer.Domain.Entities;
using PralPer.Infrastructure.Persistence;
using PralPer.Infrastructure.Seed;

namespace PralPer.Infrastructure.Services;

/// <summary>Reads/writes the competency catalog for the Attribute Administration and Level Mapping screens.</summary>
public sealed class CatalogAdminService : ICatalogAdminService
{
    private readonly AppDbContext _db;

    public CatalogAdminService(AppDbContext db) => _db = db;

    private static readonly string[] Levels = CompetencyCatalogSeeder.JobLevels;

    public async Task<IReadOnlyList<CompetencyOption>> GetCompetenciesAsync(CancellationToken ct = default)
        => await _db.Competencies.OrderBy(c => c.Name)
            .Select(c => new CompetencyOption(c.Id, c.Name)).ToListAsync(ct);

    public async Task<IReadOnlyList<LevelOption>> GetLevelsAsync(CancellationToken ct = default)
    {
        var rows = await _db.Designations.Where(d => Levels.Contains(d.Name))
            .Select(d => new LevelOption(d.Id, d.Name)).ToListAsync(ct);
        // keep the canonical Figma order (All Levels, Entry Level, Staff, ...)
        return rows.OrderBy(r => Array.IndexOf(Levels, r.Name)).ToList();
    }

    // ---- Attribute Administration ----

    public async Task<IReadOnlyList<CompetencyCountDto>> GetCompetencyCountsAsync(CancellationToken ct = default)
        => await _db.Competencies
            .Where(c => c.Attributes.Any())
            .OrderBy(c => c.Name)
            .Select(c => new CompetencyCountDto(c.Id, c.Name, c.Attributes.Count))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AttributeAdminRow>> GetAttributesAsync(int? competencyId, string? search, CancellationToken ct = default)
    {
        var q = _db.Attributes.Include(a => a.Competency).AsNoTracking().AsQueryable();
        if (competencyId is int cid)
            q = q.Where(a => a.CompetencyId == cid);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(a => a.Name.Contains(s) || (a.Description != null && a.Description.Contains(s)));
        }

        return await q
            .OrderBy(a => a.Competency!.Name).ThenBy(a => a.Name)
            .Select(a => new AttributeAdminRow(
                a.Id, a.CompetencyId, a.Competency!.Name, a.Name, a.Description, a.Weight, a.IsActive))
            .ToListAsync(ct);
    }

    public async Task<Result> CreateAttributeAsync(AttributeInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) return Result.Failure("Attribute name is required.");
        if (input.CompetencyId == 0) return Result.Failure("Please select a competency category.");
        if (input.Weight is < 0 or > 100) return Result.Failure("Weightage must be between 0 and 100.");

        var name = input.Name.Trim();
        if (await _db.Attributes.AnyAsync(a => a.CompetencyId == input.CompetencyId && a.Name == name, ct))
            return Result.Failure("That attribute already exists for this competency.");

        _db.Attributes.Add(new AttributeItem
        {
            CompetencyId = input.CompetencyId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            Weight = input.Weight,
            IsActive = input.IsActive
        });
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> SetAttributeStatusAsync(int attributeId, bool isActive, CancellationToken ct = default)
    {
        var attr = await _db.Attributes.FirstOrDefaultAsync(a => a.Id == attributeId, ct);
        if (attr is null) return Result.Failure("Attribute not found.");
        attr.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAttributeAsync(int attributeId, CancellationToken ct = default)
    {
        var attr = await _db.Attributes.FirstOrDefaultAsync(a => a.Id == attributeId, ct);
        if (attr is null) return Result.Failure("Attribute not found.");

        var maps = _db.DesignationAttributeMaps.Where(m => m.AttributeId == attributeId);
        _db.DesignationAttributeMaps.RemoveRange(maps);
        _db.Attributes.Remove(attr);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ---- Level Mapping ----

    public async Task<IReadOnlyList<LevelMapRow>> GetLevelMapsAsync(CancellationToken ct = default)
        => await _db.DesignationAttributeMaps
            .Where(m => Levels.Contains(m.Designation!.Name))
            .OrderBy(m => m.Attribute!.Competency!.Name).ThenBy(m => m.Attribute!.Name)
            .Select(m => new LevelMapRow(
                m.Id, m.Attribute!.Competency!.Name, m.Attribute!.Name, m.Designation!.Name, m.Weight))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CompetencyWeightDto>> GetLevelWeightSummaryAsync(CancellationToken ct = default)
        => await _db.DesignationAttributeMaps
            .Where(m => Levels.Contains(m.Designation!.Name))
            .GroupBy(m => m.Attribute!.Competency!.Name)
            .Select(g => new CompetencyWeightDto(g.Key, g.Sum(m => m.Weight)))
            .OrderBy(x => x.Competency)
            .ToListAsync(ct);

    public async Task<Result> CreateLevelMapAsync(int competencyId, string attributeName, int levelId, decimal weight, CancellationToken ct = default)
    {
        if (competencyId == 0) return Result.Failure("Please select a competency.");
        if (string.IsNullOrWhiteSpace(attributeName)) return Result.Failure("Please enter an attribute name.");
        if (levelId == 0) return Result.Failure("Please select a level.");
        if (weight is < 0 or > 100) return Result.Failure("Weightage must be between 0 and 100.");

        var name = attributeName.Trim();
        var attr = await _db.Attributes.FirstOrDefaultAsync(a => a.CompetencyId == competencyId && a.Name == name, ct);
        if (attr is null)
        {
            attr = new AttributeItem { CompetencyId = competencyId, Name = name, Weight = weight, IsActive = true };
            _db.Attributes.Add(attr);
            await _db.SaveChangesAsync(ct);
        }

        if (await _db.DesignationAttributeMaps.AnyAsync(m => m.DesignationId == levelId && m.AttributeId == attr.Id, ct))
            return Result.Failure("That attribute is already mapped to this level.");

        _db.DesignationAttributeMaps.Add(new DesignationAttributeMap
        {
            DesignationId = levelId, AttributeId = attr.Id, Weight = weight
        });
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> UpdateLevelMapWeightAsync(int mapId, decimal weight, CancellationToken ct = default)
    {
        if (weight is < 0 or > 100) return Result.Failure("Weightage must be between 0 and 100.");
        var map = await _db.DesignationAttributeMaps.FirstOrDefaultAsync(m => m.Id == mapId, ct);
        if (map is null) return Result.Failure("Mapping not found.");
        map.Weight = weight;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteLevelMapAsync(int mapId, CancellationToken ct = default)
    {
        var map = await _db.DesignationAttributeMaps.FirstOrDefaultAsync(m => m.Id == mapId, ct);
        if (map is null) return Result.Failure("Mapping not found.");
        _db.DesignationAttributeMaps.Remove(map);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
