using PralPer.Application.Catalog;
using PralPer.Application.Common;

namespace PralPer.Application.Services;

/// <summary>
/// Backs the manager's Attribute Administration and Level Mapping screens. Reads the competency
/// catalog (competencies, attributes, job levels, attribute→level weight maps) and persists edits.
/// </summary>
public interface ICatalogAdminService
{
    // ---- shared dropdowns (filled from tables) ----
    Task<IReadOnlyList<CompetencyOption>> GetCompetenciesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LevelOption>> GetLevelsAsync(CancellationToken ct = default);

    // ---- Attribute Administration ----
    Task<IReadOnlyList<CompetencyCountDto>> GetCompetencyCountsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AttributeAdminRow>> GetAttributesAsync(int? competencyId, string? search, CancellationToken ct = default);
    Task<Result> CreateAttributeAsync(AttributeInput input, CancellationToken ct = default);
    Task<Result> SetAttributeStatusAsync(int attributeId, bool isActive, CancellationToken ct = default);
    Task<Result> DeleteAttributeAsync(int attributeId, CancellationToken ct = default);

    // ---- Level Mapping ----
    Task<IReadOnlyList<LevelMapRow>> GetLevelMapsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CompetencyWeightDto>> GetLevelWeightSummaryAsync(CancellationToken ct = default);
    Task<Result> CreateLevelMapAsync(int competencyId, string attributeName, int levelId, decimal weight, CancellationToken ct = default);
    Task<Result> UpdateLevelMapWeightAsync(int mapId, decimal weight, CancellationToken ct = default);
    Task<Result> DeleteLevelMapAsync(int mapId, CancellationToken ct = default);
}
