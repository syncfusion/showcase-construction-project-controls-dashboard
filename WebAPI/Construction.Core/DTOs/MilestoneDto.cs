using ProjectTaskStatus = Construction.Core.Entities.TaskStatus;

namespace Construction.Core.DTOs;

public class MilestoneDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public string? Owner { get; set; }
    public int FloatDays { get; set; }
    public string? Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class MilestoneCreateDto
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public string? Owner { get; set; }
    public int FloatDays { get; set; }
    public string? Type { get; set; }
}

public class MilestoneUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public string? Owner { get; set; }
    public int FloatDays { get; set; }
    public string? Type { get; set; }
}

public class UpcomingMilestoneDto
{
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string? Owner { get; set; }
    public HealthStatus HealthStatus { get; set; }
}
