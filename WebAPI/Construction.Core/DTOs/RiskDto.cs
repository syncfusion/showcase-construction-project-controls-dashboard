using Construction.Core.Entities;

namespace Construction.Core.DTOs;

public class RiskDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RiskSeverity Severity { get; set; }
    public RiskProbability Probability { get; set; }
    public RiskImpactType ImpactType { get; set; }
    public string? ImpactDescription { get; set; }
    public decimal? ImpactCost { get; set; }
    public int? ImpactDays { get; set; }
    public string? Owner { get; set; }
    public RiskStatus Status { get; set; }
    public string? MitigationPlan { get; set; }
    public DateTime IdentifiedDate { get; set; }
    public DateTime? TargetResolutionDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public string ImpactDisplay => ImpactType switch
    {
        RiskImpactType.Cost when ImpactCost.HasValue => $"Cost · {ImpactCost.Value.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}",
        RiskImpactType.Schedule when ImpactDays.HasValue => $"Schedule · {ImpactDays.Value}d",
        RiskImpactType.Both => $"{(ImpactDays.HasValue ? $"Schedule · {ImpactDays.Value}d" : "")}{(ImpactCost.HasValue ? $" · {ImpactCost.Value.ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}" : "")}",
        RiskImpactType.Quality => "Quality",
        RiskImpactType.Safety => "Safety",
        _ => ImpactType.ToString()
    };
}

public class RiskCreateDto
{
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RiskSeverity Severity { get; set; }
    public RiskProbability Probability { get; set; }
    public RiskImpactType ImpactType { get; set; }
    public string? ImpactDescription { get; set; }
    public decimal? ImpactCost { get; set; }
    public int? ImpactDays { get; set; }
    public string? Owner { get; set; }
    public RiskStatus Status { get; set; }
    public string? MitigationPlan { get; set; }
    public DateTime IdentifiedDate { get; set; }
    public DateTime? TargetResolutionDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}

public class RiskUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RiskSeverity Severity { get; set; }
    public RiskProbability Probability { get; set; }
    public RiskImpactType ImpactType { get; set; }
    public string? ImpactDescription { get; set; }
    public decimal? ImpactCost { get; set; }
    public int? ImpactDays { get; set; }
    public string? Owner { get; set; }
    public RiskStatus Status { get; set; }
    public string? MitigationPlan { get; set; }
    public DateTime? TargetResolutionDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}

public class RiskKpisDto
{
    public int Critical { get; set; }
    public int High { get; set; }
    public int Medium { get; set; }
    public int Low { get; set; }
    public int MitigatedThisMonth { get; set; }
    public int Open { get; set; }
}

public class RiskMatrixDto
{
    public IReadOnlyList<RiskMatrixRowDto> Rows { get; set; } = new List<RiskMatrixRowDto>();
}

public class RiskMatrixRowDto
{
    public string Probability { get; set; } = string.Empty;
    public IReadOnlyList<RiskMatrixCellDto> Cells { get; set; } = new List<RiskMatrixCellDto>();
}

public class RiskMatrixCellDto
{
    public string Severity { get; set; } = string.Empty;
    public IReadOnlyList<string> RiskNumbers { get; set; } = new List<string>();
}
