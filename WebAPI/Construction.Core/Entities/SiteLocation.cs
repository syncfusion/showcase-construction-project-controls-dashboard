using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a physical location within a construction site (used for Maps)
/// </summary>
public class SiteLocation : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [MaxLength(100)]
    public string? LocationType { get; set; } // e.g., "Building", "Trailer", "Equipment"

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
}
