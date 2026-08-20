namespace Construction.Core.DTOs;

public class PortfolioKpisDto
{
    public int ActiveProjects { get; set; }
    public decimal ScheduleVariancePct { get; set; }
    public decimal CostVariancePct { get; set; }
    public decimal Cpi { get; set; }
    public decimal Spi { get; set; }
    public int OpenRisks { get; set; }
    public int CriticalRisks { get; set; }
}

public class CostPerformancePointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Planned { get; set; }
    public decimal Actual { get; set; }
}

public class ProjectHealthDistributionDto
{
    public int OnTrack { get; set; }
    public int AtRisk { get; set; }
    public int Critical { get; set; }
    public int NotStarted { get; set; }
    public int Total => OnTrack + AtRisk + Critical + NotStarted;
}

public class CostKpisDto
{
    public decimal TotalPortfolioBudget { get; set; }
    public decimal CommittedSpend { get; set; }
    public decimal ForecastAtCompletion { get; set; }
    public decimal PendingChangeOrdersAmount { get; set; }
    public int PendingChangeOrdersCount { get; set; }
}

public class EarnedValuePointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Bcws { get; set; }
    public decimal Bcwp { get; set; }
    public decimal Acwp { get; set; }
}

public class CostVarianceByCostCodeDto
{
    public string CostCode { get; set; } = string.Empty;
    public decimal VariancePct { get; set; }
}
