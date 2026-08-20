namespace Construction.Core.DTOs;

/// <summary>
/// DTO for scheduler appointments
/// </summary>
public class AppointmentDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for chart data points
/// </summary>
public class ChartDataPointDto
{
    public string X { get; set; } = string.Empty;
    public int Y { get; set; }
}

/// <summary>
/// DTO for map locations
/// </summary>
public class MapLocationDto
{
    public int ProjectId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? Color { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Progress { get; set; }
    public string? Health { get; set; }
    public IReadOnlyList<string> TopIssues { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Health indicator for a map location/project
/// </summary>
public enum ProjectHealth
{
    Good,
    AtRisk,
    Critical
}

/// <summary>
/// DTO for diagram nodes
/// </summary>
public class DiagramNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class DiagramConnectorDto
{
    public string Id { get; set; } = string.Empty;
    public string SourceID { get; set; } = string.Empty;
    public string TargetID { get; set; } = string.Empty;
}
