namespace Construction.Maui.Models;

/// <summary>Enumeration mirrors Construction.Core.Entities.ProjectStatus exactly.</summary>
public enum ProjectStatus
{
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5,
}

public enum HealthStatus
{
    NotStarted = 0,
    OnTrack,
    AtRisk,
    Critical,
}

public enum TaskStatus
{
    NotStarted = 1,
    InProgress,
    OnHold,
    Completed,
    Cancelled,
}

public enum RiskSeverity
{
    Low = 1,
    Medium,
    High,
    Critical,
}

public enum RiskProbability
{
    Low = 1,
    Medium,
    High,
}

public enum RiskImpactType
{
    Schedule = 1,
    Cost,
    Both,
    Quality,
    Safety,
}

public enum RiskStatus
{
    Open = 1,
    InProgress,
    Monitoring,
    Escalated,
    Containment,
    Mitigated,
    Closed,
}
