using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface IRfiService
{
    Task<PagedResponseDto<RfiDto>> GetRfisAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<RfiDto?> GetRfiByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<RfiSummaryDto>> GetRfisByProjectAsync(int projectId, CancellationToken ct = default);
}
