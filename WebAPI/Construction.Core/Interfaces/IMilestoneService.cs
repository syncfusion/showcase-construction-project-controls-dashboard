using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface IMilestoneService
{
    Task<PagedResponseDto<MilestoneDto>> GetMilestonesAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<MilestoneDto?> GetMilestoneByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MilestoneDto>> GetMilestonesByProjectAsync(int projectId, int days, CancellationToken ct = default);
    Task<IReadOnlyList<MilestoneDto>> GetUpcomingMilestonesAsync(int days, int limit, CancellationToken ct = default);
}
