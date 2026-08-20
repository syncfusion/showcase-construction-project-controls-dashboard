using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

public interface IReportService
{
    Task<PortfolioKpisDto> GetPortfolioKpisAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CostPerformancePointDto>> GetCostPerformanceTrendAsync(int months, CancellationToken ct = default);
    Task<ProjectHealthDistributionDto> GetProjectHealthDistributionAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UpcomingMilestoneDto>> GetUpcomingMilestonesAsync(int days, int limit, CancellationToken ct = default);
    Task<CostKpisDto> GetCostKpisAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EarnedValuePointDto>> GetEarnedValueTrendAsync(int months, CancellationToken ct = default);
    Task<IReadOnlyList<CostVarianceByCostCodeDto>> GetCostVarianceByCostCodeAsync(CancellationToken ct = default);
}
