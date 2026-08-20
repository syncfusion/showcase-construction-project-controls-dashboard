using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface IInspectionService
{
    Task<PagedResponseDto<InspectionDto>> GetInspectionsAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<InspectionDto?> GetInspectionByIdAsync(int id, CancellationToken ct = default);
}
