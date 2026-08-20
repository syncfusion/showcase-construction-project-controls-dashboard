using Construction.Core.DTOs;

namespace Construction.Blazor.Core.Services;

public class ProjectsService(ApiClient api)
{
    public Task<PagedResponseDto<ProjectDto>> GetProjectsAsync(int page = 1, int pageSize = 50) =>
        api.GetJsonAsync<PagedResponseDto<ProjectDto>>("projects", new Dictionary<string, object?> { ["page"] = page, ["pageSize"] = pageSize });

    public Task<ProjectDto> GetByIdAsync(int id) =>
        api.GetJsonAsync<ProjectDto>($"projects/{id}");

    public Task<ProjectKpisDto> GetKpisAsync(int id) =>
        api.GetJsonAsync<ProjectKpisDto>($"projects/{id}/kpis");

    public Task<List<RiskDto>> GetTopRisksAsync(int id, int limit = 5) =>
        api.GetJsonAsync<List<RiskDto>>($"projects/{id}/top-open-risks", new Dictionary<string, object?> { ["limit"] = limit });

    // Distinct from ReportsService.GetUpcomingMilestonesAsync (portfolio-wide UpcomingMilestoneDto) —
    // this per-project endpoint returns MilestoneDto (own status/plannedDate/owner), not project health.
    public Task<List<MilestoneDto>> GetUpcomingMilestonesAsync(int id, int days = 60, int limit = 10) =>
        api.GetJsonAsync<List<MilestoneDto>>($"projects/{id}/upcoming-milestones", new Dictionary<string, object?> { ["days"] = days, ["limit"] = limit });

    public Task<List<RecentDocumentDto>> GetRecentDocumentsAsync(int id, int days = 90, int limit = 10) =>
        api.GetJsonAsync<List<RecentDocumentDto>>($"projects/{id}/recent-documents", new Dictionary<string, object?> { ["days"] = days, ["limit"] = limit });

    public Task<List<RfiSummaryDto>> GetRfisAsync(int id, int limit = 50) =>
        api.GetJsonAsync<List<RfiSummaryDto>>($"projects/{id}/rfis", new Dictionary<string, object?> { ["limit"] = limit });

    public Task<List<SubmittalSummaryDto>> GetSubmittalsAsync(int id, int limit = 50) =>
        api.GetJsonAsync<List<SubmittalSummaryDto>>($"projects/{id}/submittals", new Dictionary<string, object?> { ["limit"] = limit });

    public Task<List<ChangeOrderSummaryDto>> GetChangeOrdersAsync(int id, int limit = 50) =>
        api.GetJsonAsync<List<ChangeOrderSummaryDto>>($"projects/{id}/change-orders", new Dictionary<string, object?> { ["limit"] = limit });

    public Task<List<MapLocationDto>> GetLocationsAsync() =>
        api.GetJsonAsync<List<MapLocationDto>>("projects/locations");
}
