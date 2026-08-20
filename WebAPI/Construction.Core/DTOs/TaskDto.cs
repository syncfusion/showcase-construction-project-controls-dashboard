namespace Construction.Core.DTOs;

/// <summary>
/// DTO for ProjectTask entity (read operations) - optimized for Gantt Chart
/// </summary>
public class TaskDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Entities.TaskStatus Status { get; set; }
    public int Progress { get; set; }
    public string? AssignedTo { get; set; }
    public int? ParentTaskId { get; set; }
    public string? Dependencies { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

/// <summary>
/// DTO for creating a new ProjectTask
/// </summary>
public class TaskCreateDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Entities.TaskStatus Status { get; set; }
    public int Progress { get; set; }
    public string? AssignedTo { get; set; }
    public int? ParentTaskId { get; set; }
    public string? Dependencies { get; set; }
}

/// <summary>
/// DTO for updating an existing ProjectTask
/// </summary>
public class TaskUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Entities.TaskStatus Status { get; set; }
    public int Progress { get; set; }
    public string? AssignedTo { get; set; }
    public int? ParentTaskId { get; set; }
    public string? Dependencies { get; set; }
}
