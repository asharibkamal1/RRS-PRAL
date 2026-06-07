using PralPer.Application.Manager;

namespace PralPer.Application.Services;

/// <summary>Fetches the manager dashboard snapshot, a rater's ratees, and a read-only 360 detail.</summary>
public interface IManagerDashboardService
{
    Task<ManagerHomeDto> GetHomeAsync(CancellationToken ct = default);
    Task<RaterEvaluationsDto?> GetRaterEvaluationsAsync(int raterId, CancellationToken ct = default);
    Task<PeerEvaluationDetailDto?> GetEvaluationDetailAsync(int snapshotId, CancellationToken ct = default);
}
