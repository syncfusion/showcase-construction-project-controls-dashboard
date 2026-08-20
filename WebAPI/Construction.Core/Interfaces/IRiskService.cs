using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface IRiskService
{
    Task<PagedResponseDto<RiskDto>> GetRisksAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<RiskDto?> GetRiskByIdAsync(int id, CancellationToken ct = default);
    Task<RiskKpisDto> GetKpisAsync(CancellationToken ct = default);
    Task<RiskMatrixDto> GetMatrixAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RiskDto>> GetTopOpenRisksByProjectAsync(int projectId, int limit, CancellationToken ct = default);
}
