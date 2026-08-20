using Construction.Core.DTOs;

namespace Construction.Blazor.Core.Services;

public class ReportsService(ApiClient api)
{
    public Task<PortfolioKpisDto> GetPortfolioKpisAsync() =>
        api.GetJsonAsync<PortfolioKpisDto>("reports/portfolio-kpis");

    public Task<List<CostPerformancePointDto>> GetCostPerformanceTrendAsync(int months = 12) =>
        api.GetJsonAsync<List<CostPerformancePointDto>>("reports/cost-performance-trend", new Dictionary<string, object?> { ["months"] = months });

    public Task<ProjectHealthDistributionDto> GetProjectHealthDistributionAsync() =>
        api.GetJsonAsync<ProjectHealthDistributionDto>("reports/project-health-distribution");

    public Task<List<UpcomingMilestoneDto>> GetUpcomingMilestonesAsync(int days = 30, int limit = 20) =>
        api.GetJsonAsync<List<UpcomingMilestoneDto>>("reports/upcoming-milestones", new Dictionary<string, object?> { ["days"] = days, ["limit"] = limit });

    public Task<CostKpisDto> GetCostKpisAsync() =>
        api.GetJsonAsync<CostKpisDto>("reports/cost-kpis");

    public Task<List<EarnedValuePointDto>> GetEarnedValueTrendAsync(int months = 12) =>
        api.GetJsonAsync<List<EarnedValuePointDto>>("reports/earned-value-trend", new Dictionary<string, object?> { ["months"] = months });

    public Task<List<CostVarianceByCostCodeDto>> GetCostVarianceByCostCodeAsync() =>
        api.GetJsonAsync<List<CostVarianceByCostCodeDto>>("reports/cost-variance-by-cost-code");
}
