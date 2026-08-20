namespace Construction.Core.DTOs;

public class ProjectKpisDto
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

public class RecentDocumentDto
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
