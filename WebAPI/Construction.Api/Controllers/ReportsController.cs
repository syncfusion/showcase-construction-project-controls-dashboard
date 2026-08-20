using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    [HttpGet("portfolio-kpis")]
    public async Task<ActionResult<PortfolioKpisDto>> GetPortfolioKpis(CancellationToken ct)
        => Ok(await _reports.GetPortfolioKpisAsync(ct));

    [HttpGet("cost-performance-trend")]
    public async Task<ActionResult<IReadOnlyList<CostPerformancePointDto>>> GetCostPerformanceTrend([FromQuery] int months = 12, CancellationToken ct = default)
        => Ok(await _reports.GetCostPerformanceTrendAsync(months, ct));

    [HttpGet("project-health-distribution")]
    public async Task<ActionResult<ProjectHealthDistributionDto>> GetProjectHealthDistribution(CancellationToken ct)
        => Ok(await _reports.GetProjectHealthDistributionAsync(ct));

    [HttpGet("upcoming-milestones")]
    public async Task<ActionResult<IReadOnlyList<UpcomingMilestoneDto>>> GetUpcomingMilestones([FromQuery] int days = 30, [FromQuery] int limit = 20, CancellationToken ct = default)
        => Ok(await _reports.GetUpcomingMilestonesAsync(days, limit, ct));

    [HttpGet("cost-kpis")]
    public async Task<ActionResult<CostKpisDto>> GetCostKpis(CancellationToken ct)
        => Ok(await _reports.GetCostKpisAsync(ct));

    [HttpGet("earned-value-trend")]
    public async Task<ActionResult<IReadOnlyList<EarnedValuePointDto>>> GetEarnedValueTrend([FromQuery] int months = 12, CancellationToken ct = default)
        => Ok(await _reports.GetEarnedValueTrendAsync(months, ct));

    [HttpGet("cost-variance-by-cost-code")]
    public async Task<ActionResult<IReadOnlyList<CostVarianceByCostCodeDto>>> GetCostVarianceByCostCode(CancellationToken ct)
        => Ok(await _reports.GetCostVarianceByCostCodeAsync(ct));
}
