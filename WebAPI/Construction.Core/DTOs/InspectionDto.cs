using Construction.Core.Entities;

namespace Construction.Core.DTOs;

public class InspectionDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int? LocationId { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public InspectionStatus Status { get; set; }
    public string? Inspector { get; set; }
    public string? Notes { get; set; }
    public string? Findings { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class InspectionCreateDto
{
    public int ProjectId { get; set; }
    public int? LocationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public InspectionStatus Status { get; set; }
    public string? Inspector { get; set; }
}

public class InspectionUpdateDto
{
    public string Type { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public InspectionStatus Status { get; set; }
    public string? Inspector { get; set; }
    public string? Notes { get; set; }
    public string? Findings { get; set; }
}
