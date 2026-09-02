using Construction.Maui.Models;

namespace Construction.Maui.Services;

// ============================================================================
//  JSON-backed service implementations
// ----------------------------------------------------------------------------
//  These implement the SAME service interfaces (IReportsService,
//  IProjectsService, IRisksService) used by the existing ViewModels, but read
//  the pre-extracted JSON files from the Data/ folder via LocalDataService
//  instead of calling the backend API over HTTP. This lets the showcase
//  dashboard render with real data WITHOUT requiring the API (or any DB) to be
//  running at runtime.
//
//  The original HTTP-backed service classes (ReportsService, ProjectsService,
//  RisksService) are preserved below unchanged — only the DI registrations in
//  MauiProgram.cs were swapped to these JSON-backed ones (the old registrations
//  are commented out there with a summary). To switch back to live API calls,
//  restore the old registrations and remove these from DI.
// ============================================================================

/// <summary>JSON-backed implementation of IReportsService — reads the report
/// JSON files (portfolio KPIs, cost KPIs, health distribution, trends) from
/// Data/ via LocalDataService. Mirrors the ReportsService HTTP methods
/// one-for-one so the ViewModels need no changes.</summary>
public sealed class LocalReportsService : IReportsService
{
    private readonly ILocalDataService _data;
    public LocalReportsService(ILocalDataService data) => _data = data;

    public Task<PortfolioKpisDto> GetPortfolioKpisAsync(CancellationToken ct = default)
        => _data.GetPortfolioKpisAsync(ct);

    public Task<CostKpisDto> GetCostKpisAsync(CancellationToken ct = default)
        => _data.GetCostKpisAsync(ct);

    public Task<ProjectHealthDistributionDto> GetProjectHealthDistributionAsync(CancellationToken ct = default)
        => _data.GetProjectHealthDistributionAsync(ct);

    public Task<List<CostPerformancePointDto>> GetCostPerformanceTrendAsync(int months = 6, CancellationToken ct = default)
        => _data.GetCostPerformanceTrendAsync(12, ct);

    public Task<List<UpcomingMilestoneDto>> GetUpcomingMilestonesAsync(int days = 14, int limit = 10, CancellationToken ct = default)
        => _data.GetPortfolioUpcomingMilestonesAsync(365, 200, ct);

    public Task<List<EarnedValuePointDto>> GetEarnedValueTrendAsync(int months = 12, CancellationToken ct = default)
        => _data.GetEarnedValueTrendAsync(12, ct);

    public Task<List<CostVarianceByCostCodeDto>> GetCostVarianceByCostCodeAsync(CancellationToken ct = default)
        => _data.GetCostVarianceByCostCodeAsync(ct);
}

/// <summary>JSON-backed implementation of IProjectsService — reads the project
/// list + per-project detail JSON files from Data/projects/ via
/// LocalDataService. Mirrors the ProjectsService HTTP methods one-for-one so
/// the ViewModels (Dashboard, Projects, ProjectDetail) need no changes.</summary>
public sealed class LocalProjectsService : IProjectsService
{
    private readonly ILocalDataService _data;
    public LocalProjectsService(ILocalDataService data) => _data = data;

    public Task<PagedResponse<ProjectDto>> GetProjectsAsync(QueryParameters query, CancellationToken ct = default)
        => _data.GetProjectsAsync(query, ct);

    public Task<ProjectDto?> GetProjectAsync(int id, CancellationToken ct = default)
        => _data.GetProjectAsync(id, ct);

    public Task<ProjectKpisDto?> GetProjectKpisAsync(int id, CancellationToken ct = default)
        => _data.GetProjectKpisAsync(id, ct);

    public Task<List<TaskDto>> GetProjectTasksAsync(int id, QueryParameters query, CancellationToken ct = default)
        => _data.GetProjectTasksAsync(id, query, ct);

    public Task<List<RecentDocumentDto>> GetRecentDocumentsAsync(int id, int days = 30, int limit = 10, CancellationToken ct = default)
        => _data.GetRecentDocumentsAsync(id, 365, Math.Max(limit, 50), ct);

    public Task<List<RiskDto>> GetTopOpenRisksAsync(int id, int limit = 5, CancellationToken ct = default)
        => _data.GetTopOpenRisksAsync(id, Math.Max(limit, 20), ct);

    public Task<List<MilestoneDto>> GetUpcomingMilestonesAsync(int id, int days = 30, CancellationToken ct = default)
        => _data.GetUpcomingMilestonesAsync(id, 365, ct);

    public Task<List<RfiSummaryDto>> GetProjectRfisAsync(int id, CancellationToken ct = default)
        => _data.GetProjectRfisAsync(id, ct);

    public Task<List<SubmittalSummaryDto>> GetProjectSubmittalsAsync(int id, CancellationToken ct = default)
        => _data.GetProjectSubmittalsAsync(id, ct);

    public Task<List<ChangeOrderSummaryDto>> GetProjectChangeOrdersAsync(int id, CancellationToken ct = default)
        => _data.GetProjectChangeOrdersAsync(id, ct);
}

/// <summary>JSON-backed implementation of IRisksService — reads the risks +
/// budgets JSON files from Data/ via LocalDataService. Mirrors the RisksService
/// HTTP methods one-for-one so the RisksViewModel + CostControlViewModel need
/// no changes.</summary>
public sealed class LocalRisksService : IRisksService
{
    private readonly ILocalDataService _data;
    public LocalRisksService(ILocalDataService data) => _data = data;

    public Task<PagedResponse<RiskDto>> GetRisksAsync(QueryParameters query, CancellationToken ct = default)
        => _data.GetRisksAsync(query, ct);

    public Task<List<BudgetDto>> GetBudgetsAsync(QueryParameters query, CancellationToken ct = default)
        => _data.GetBudgetsAsync(query, ct);
}
