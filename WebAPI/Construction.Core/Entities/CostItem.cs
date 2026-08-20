using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents an individual cost item within a budget category
/// </summary>
public class CostItem : BaseEntity
{
    [Required]
    public int BudgetId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(100)]
    public string? Vendor { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; } // Invoice number, PO number, etc.

    // Navigation properties
    public Budget Budget { get; set; } = null!;
}
