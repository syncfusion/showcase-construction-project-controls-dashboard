using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

public interface IProjectService
{
    Task<PagedResponseDto<ProjectDto>> GetProjectsAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<ProjectDto?> GetProjectByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResponseDto<TaskDto>> GetProjectTasksAsync(int projectId, QueryParametersDto query, CancellationToken ct = default);
    Task<IReadOnlyList<MapLocationDto>> GetProjectLocationsAsync(CancellationToken ct = default);
    Task<ProjectKpisDto> GetProjectKpisAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<RecentDocumentDto>> GetRecentDocumentsAsync(int id, int days, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<RiskDto>> GetTopOpenRisksAsync(int id, int limit, CancellationToken ct = default);
    Task<IReadOnlyList<MilestoneDto>> GetUpcomingMilestonesAsync(int id, int days, CancellationToken ct = default);
    Task<IReadOnlyList<RfiSummaryDto>> GetProjectRfisAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SubmittalSummaryDto>> GetProjectSubmittalsAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<ChangeOrderSummaryDto>> GetProjectChangeOrdersAsync(int id, CancellationToken ct = default);
}
