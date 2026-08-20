using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface IBudgetService
{
    Task<PagedResponseDto<BudgetDto>> GetBudgetsAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<BudgetDto?> GetBudgetByIdAsync(int id, CancellationToken ct = default);
}
