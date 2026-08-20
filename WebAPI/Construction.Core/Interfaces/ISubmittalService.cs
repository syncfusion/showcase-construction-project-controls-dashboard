using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface ISubmittalService
{
    Task<PagedResponseDto<SubmittalDto>> GetSubmittalsAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<SubmittalDto?> GetSubmittalByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SubmittalSummaryDto>> GetSubmittalsByProjectAsync(int projectId, CancellationToken ct = default);
}
