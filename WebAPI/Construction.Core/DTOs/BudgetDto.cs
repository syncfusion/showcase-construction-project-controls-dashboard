namespace Construction.Core.DTOs;

public class CostItemDto
{
    public int Id { get; set; }
    public int BudgetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? Date { get; set; }
    public string? Vendor { get; set; }
    public string? Reference { get; set; }
}

public class BudgetDto
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
    public ICollection<CostItemDto> CostItems { get; set; } = new List<CostItemDto>();
}

public class BudgetCreateDto
{
    public int ProjectId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public class BudgetUpdateDto
{
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
}
