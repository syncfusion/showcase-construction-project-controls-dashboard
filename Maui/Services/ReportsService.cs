using Construction.Maui.Models;

namespace Construction.Maui.Services;

/// <summary>Reports endpoint mirror (portfolio KPIs, cost, health distribution, trends).</summary>
public interface IReportsService
{
    Task<PortfolioKpisDto> GetPortfolioKpisAsync(CancellationToken ct = default);
    Task<CostKpisDto> GetCostKpisAsync(CancellationToken ct = default);
    Task<ProjectHealthDistributionDto> GetProjectHealthDistributionAsync(CancellationToken ct = default);
    Task<List<CostPerformancePointDto>> GetCostPerformanceTrendAsync(int months = 6, CancellationToken ct = default);
    Task<List<UpcomingMilestoneDto>> GetUpcomingMilestonesAsync(int days = 14, int limit = 10, CancellationToken ct = default);
    Task<List<EarnedValuePointDto>> GetEarnedValueTrendAsync(int months = 12, CancellationToken ct = default);
    Task<List<CostVarianceByCostCodeDto>> GetCostVarianceByCostCodeAsync(CancellationToken ct = default);
}

public sealed class ReportsService : IReportsService
{
    private readonly IApiClient _api;
    public ReportsService(IApiClient api) => _api = api;

    public Task<PortfolioKpisDto> GetPortfolioKpisAsync(CancellationToken ct = default)
        => _api.GetJsonAsync<PortfolioKpisDto>("reports/portfolio-kpis", ct);

    public Task<CostKpisDto> GetCostKpisAsync(CancellationToken ct = default)
        => _api.GetJsonAsync<CostKpisDto>("reports/cost-kpis", ct);

    public Task<ProjectHealthDistributionDto> GetProjectHealthDistributionAsync(CancellationToken ct = default)
        => _api.GetJsonAsync<ProjectHealthDistributionDto>("reports/project-health-distribution", ct);

    public Task<List<CostPerformancePointDto>> GetCostPerformanceTrendAsync(int months = 6, CancellationToken ct = default)
        => _api.GetJsonAsync<List<CostPerformancePointDto>>($"reports/cost-performance-trend?months={months}", ct);

    public Task<List<UpcomingMilestoneDto>> GetUpcomingMilestonesAsync(int days = 14, int limit = 10, CancellationToken ct = default)
        => _api.GetJsonAsync<List<UpcomingMilestoneDto>>($"reports/upcoming-milestones?days={days}&limit={limit}", ct);

    public Task<List<EarnedValuePointDto>> GetEarnedValueTrendAsync(int months = 12, CancellationToken ct = default)
        => _api.GetJsonAsync<List<EarnedValuePointDto>>($"reports/earned-value-trend?months={months}", ct);

    public Task<List<CostVarianceByCostCodeDto>> GetCostVarianceByCostCodeAsync(CancellationToken ct = default)
        => _api.GetJsonAsync<List<CostVarianceByCostCodeDto>>("reports/cost-variance-by-cost-code", ct);
}
