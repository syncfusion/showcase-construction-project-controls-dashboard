using Construction.Core.Entities;

namespace Construction.Core.DTOs;

public class ChangeOrderDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ChangeOrderStatus Status { get; set; }
    public string? RequestedBy { get; set; }
    public DateTime? RequestDate { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Justification { get; set; }
    public int? ImpactDays { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class ChangeOrderSummaryDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ChangeOrderStatus Status { get; set; }
    public string? RequestedBy { get; set; }
    public DateTime? RequestDate { get; set; }
}

public class ChangeOrderCreateDto
{
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ChangeOrderStatus Status { get; set; }
    public string? RequestedBy { get; set; }
    public DateTime? RequestDate { get; set; }
    public string? Justification { get; set; }
    public int? ImpactDays { get; set; }
}

public class ChangeOrderUpdateDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public ChangeOrderStatus Status { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? Justification { get; set; }
    public int? ImpactDays { get; set; }
}
