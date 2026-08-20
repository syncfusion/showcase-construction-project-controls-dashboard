using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface IChangeOrderService
{
    Task<PagedResponseDto<ChangeOrderDto>> GetChangeOrdersAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<ChangeOrderDto?> GetChangeOrderByIdAsync(int id, CancellationToken ct = default);
}
