using System.Text.Json.Serialization;

namespace Construction.Maui.Models;

/// <summary>
/// Single consolidated DTO mirror for the MAUI client.
/// All shapes here match the backend Construction.Core DTOs (camelCase over the wire,
/// see MauiProgram.JsonOptions). Backend is the source of truth — DO NOT drift.
/// </summary>
public sealed class PagedResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

public sealed class QueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Sort { get; set; }        // e.g. "name:asc"
    public string? Filter { get; set; }      // key=value pairs

    public string ToQueryString()
    {
        var q = new List<string>(4);
        if (Page > 1) q.Add($"page={Page}");
        if (PageSize != 50) q.Add($"pageSize={PageSize}");
        if (!string.IsNullOrWhiteSpace(Search)) q.Add($"search={Uri.EscapeDataString(Search!)}");
        if (!string.IsNullOrWhiteSpace(Status)) q.Add($"status={Uri.EscapeDataString(Status!)}");
        if (!string.IsNullOrWhiteSpace(Sort)) q.Add($"sort={Uri.EscapeDataString(Sort!)}");
        if (!string.IsNullOrWhiteSpace(Filter)) q.Add($"filter={Uri.EscapeDataString(Filter!)}");
        return q.Count == 0 ? string.Empty : "?" + string.Join('&', q);
    }
}

public sealed class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public string? Location { get; set; }
    public decimal Budget { get; set; }
    public int Progress { get; set; }
    public string? Manager { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public HealthStatus HealthStatus { get; set; }
}

public sealed class ProjectKpisDto
{
    public int PercentComplete { get; set; }
    public decimal CostVariance { get; set; }
    public decimal ScheduleVariance { get; set; }
    public int OpenRfis { get; set; }
    public int OverdueRfis { get; set; }
    public decimal Budget { get; set; }
    public decimal Spent { get; set; }
    public int OpenChangeOrders { get; set; }
    public int OpenRisks { get; set; }
    public int OpenSubmittals { get; set; }
    public int OverdueSubmittals { get; set; }
    public decimal PendingChangeOrdersAmount { get; set; }
    public int ApprovedSubmittals { get; set; }
}

public sealed class TaskDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TaskStatus Status { get; set; }
    public int Progress { get; set; }
    public string? AssignedTo { get; set; }
    public int? ParentTaskId { get; set; }
    public string? Dependencies { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public sealed class RiskDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RiskSeverity Severity { get; set; }
    public RiskProbability Probability { get; set; }
    public RiskImpactType ImpactType { get; set; }
    public string? ImpactDescription { get; set; }
    public decimal? ImpactCost { get; set; }
    public int? ImpactDays { get; set; }
    public string? Owner { get; set; }
    public RiskStatus Status { get; set; }
    public string? MitigationPlan { get; set; }
    public DateTime IdentifiedDate { get; set; }
    public DateTime? TargetResolutionDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    /// <summary>Client-side computed impact summary (mirrors backend ImpactDisplay).</summary>
    public string ImpactDisplay => ImpactType switch
    {
        RiskImpactType.Cost when ImpactCost.HasValue => $"Cost · {ImpactCost.Value:C0}",
        RiskImpactType.Schedule when ImpactDays.HasValue => $"Schedule · {ImpactDays.Value}d",
        RiskImpactType.Both => $"{(ImpactDays.HasValue ? $"Schedule · {ImpactDays.Value}d" : "")}{(ImpactCost.HasValue ? $" · {ImpactCost.Value:C0}" : "")}".Trim(),
        RiskImpactType.Quality => "Quality",
        RiskImpactType.Safety => "Safety",
        _ => ImpactType.ToString(),
    };
}

public sealed class BudgetDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal Remaining => AllocatedAmount - SpentAmount;
    public decimal PercentUsed => AllocatedAmount > 0 ? (SpentAmount / AllocatedAmount) * 100 : 0;
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public List<CostItemDto> CostItems { get; set; } = new();
}

public sealed class CostItemDto
{
    public int Id { get; set; }
    public int BudgetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? Date { get; set; }
    public string? Vendor { get; set; }
    public string? Reference { get; set; }
}

public sealed class RecentDocumentDto
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Document / Submittal / RFI
    public string? Revision { get; set; }
    public DateTime SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ProjectId { get; set; }
}

public sealed class MilestoneDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public TaskStatus Status { get; set; }
    public string? Owner { get; set; }
    public int FloatDays { get; set; }
    public string? Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public sealed class UpcomingMilestoneDto
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string? Owner { get; set; }
    public HealthStatus HealthStatus { get; set; }
}

public sealed class RfiSummaryDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Discipline { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? DueDate => SubmittedDate?.AddDays(14);
    public string? AssignedTo { get; set; }
    public string Status { get; set; } = string.Empty; // RFIStatus comes over the wire as string
}

public sealed class SubmittalSummaryDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Discipline { get; set; }
    public string? SubmittalType { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? DueDate => SubmittedDate?.AddDays(21);
    public string? ReviewedBy { get; set; }
    public string Status { get; set; } = string.Empty; // SubmittalStatus as string
}

public sealed class ChangeOrderSummaryDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty; // ChangeOrderStatus as string
    public string? RequestedBy { get; set; }
    public DateTime? RequestDate { get; set; }
}

// ---- Reports ----

public sealed class PortfolioKpisDto
{
    public int ActiveProjects { get; set; }
    public decimal ScheduleVariancePct { get; set; }
    public decimal CostVariancePct { get; set; }
    public decimal Cpi { get; set; }
    public decimal Spi { get; set; }
    public int OpenRisks { get; set; }
    public int CriticalRisks { get; set; }
}

public sealed class CostPerformancePointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Planned { get; set; }
    public decimal Actual { get; set; }
}

public sealed class ProjectHealthDistributionDto
{
    public int OnTrack { get; set; }
    public int AtRisk { get; set; }
    public int Critical { get; set; }
    public int NotStarted { get; set; }
    public int Total => OnTrack + AtRisk + Critical + NotStarted;
}

public sealed class CostKpisDto
{
    public decimal TotalPortfolioBudget { get; set; }
    public decimal CommittedSpend { get; set; }
    public decimal ForecastAtCompletion { get; set; }
    public decimal PendingChangeOrdersAmount { get; set; }
    public int PendingChangeOrdersCount { get; set; }
}

public sealed class EarnedValuePointDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Bcws { get; set; }
    public decimal Bcwp { get; set; }
    public decimal Acwp { get; set; }
}

public sealed class CostVarianceByCostCodeDto
{
    public string CostCode { get; set; } = string.Empty;
    public decimal VariancePct { get; set; }
}
