using Construction.Maui.Models;

namespace Construction.Maui.Services;

/// <summary>Project management endpoints (mirrors backend ProjectsController).</summary>
public interface IProjectsService
{
    Task<PagedResponse<ProjectDto>> GetProjectsAsync(QueryParameters query, CancellationToken ct = default);
    Task<ProjectDto?> GetProjectAsync(int id, CancellationToken ct = default);
    Task<ProjectKpisDto?> GetProjectKpisAsync(int id, CancellationToken ct = default);
    Task<List<TaskDto>> GetProjectTasksAsync(int id, QueryParameters query, CancellationToken ct = default);
    Task<List<RecentDocumentDto>> GetRecentDocumentsAsync(int id, int days = 30, int limit = 10, CancellationToken ct = default);
    Task<List<RiskDto>> GetTopOpenRisksAsync(int id, int limit = 5, CancellationToken ct = default);
    Task<List<MilestoneDto>> GetUpcomingMilestonesAsync(int id, int days = 30, CancellationToken ct = default);
    Task<List<RfiSummaryDto>> GetProjectRfisAsync(int id, CancellationToken ct = default);
    Task<List<SubmittalSummaryDto>> GetProjectSubmittalsAsync(int id, CancellationToken ct = default);
    Task<List<ChangeOrderSummaryDto>> GetProjectChangeOrdersAsync(int id, CancellationToken ct = default);
}

public sealed class ProjectsService : IProjectsService
{
    private readonly IApiClient _api;
    public ProjectsService(IApiClient api) => _api = api;

    public Task<PagedResponse<ProjectDto>> GetProjectsAsync(QueryParameters query, CancellationToken ct = default)
        => _api.GetJsonAsync<PagedResponse<ProjectDto>>("projects" + query.ToQueryString(), ct);

    public Task<ProjectDto?> GetProjectAsync(int id, CancellationToken ct = default)
        => _api.GetJsonAsync<ProjectDto>($"projects/{id}", ct);

    public Task<ProjectKpisDto?> GetProjectKpisAsync(int id, CancellationToken ct = default)
        => _api.GetJsonAsync<ProjectKpisDto>($"projects/{id}/kpis", ct);

    public Task<List<TaskDto>> GetProjectTasksAsync(int id, QueryParameters query, CancellationToken ct = default)
        => _api.GetJsonAsync<List<TaskDto>>($"projects/{id}/tasks" + query.ToQueryString(), ct);

    public Task<List<RecentDocumentDto>> GetRecentDocumentsAsync(int id, int days = 30, int limit = 10, CancellationToken ct = default)
        => _api.GetJsonAsync<List<RecentDocumentDto>>($"projects/{id}/recent-documents?days={days}&limit={limit}", ct);

    public Task<List<RiskDto>> GetTopOpenRisksAsync(int id, int limit = 5, CancellationToken ct = default)
        => _api.GetJsonAsync<List<RiskDto>>($"projects/{id}/top-open-risks?limit={limit}", ct);

    public Task<List<MilestoneDto>> GetUpcomingMilestonesAsync(int id, int days = 30, CancellationToken ct = default)
        => _api.GetJsonAsync<List<MilestoneDto>>($"projects/{id}/upcoming-milestones?days={days}", ct);

    public Task<List<RfiSummaryDto>> GetProjectRfisAsync(int id, CancellationToken ct = default)
        => _api.GetJsonAsync<List<RfiSummaryDto>>($"projects/{id}/rfis", ct);

    public Task<List<SubmittalSummaryDto>> GetProjectSubmittalsAsync(int id, CancellationToken ct = default)
        => _api.GetJsonAsync<List<SubmittalSummaryDto>>($"projects/{id}/submittals", ct);

    public Task<List<ChangeOrderSummaryDto>> GetProjectChangeOrdersAsync(int id, CancellationToken ct = default)
        => _api.GetJsonAsync<List<ChangeOrderSummaryDto>>($"projects/{id}/change-orders", ct);
}
