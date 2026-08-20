using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface ITaskService
{
    Task<PagedResponseDto<TaskDto>> GetTasksAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<TaskDto?> GetTaskByIdAsync(int id, CancellationToken ct = default);
}
