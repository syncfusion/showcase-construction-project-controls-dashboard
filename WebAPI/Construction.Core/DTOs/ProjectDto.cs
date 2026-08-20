using Construction.Core.Entities;

namespace Construction.Core.DTOs;

/// <summary>
/// DTO for Project entity (read operations)
/// </summary>
public class ProjectDto
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

/// <summary>
/// DTO for creating a new Project
/// </summary>
public class ProjectCreateDto
{
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
}

/// <summary>
/// DTO for updating an existing Project
/// </summary>
public class ProjectUpdateDto
{
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
}
