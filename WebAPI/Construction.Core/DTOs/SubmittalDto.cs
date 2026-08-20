using System.Text.Json.Serialization;
using Construction.Core.Entities;

namespace Construction.Core.DTOs;

public class SubmittalDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubmittalStatus Status { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? Comments { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate => SubmittedDate?.AddDays(21);

    public string? Discipline { get; set; }
    public string? SpecificationSection { get; set; }
    public string? SubmittalType { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class SubmittalSummaryDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Discipline { get; set; }
    public string? SubmittalType { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? DueDate => SubmittedDate?.AddDays(21);
    public string? ReviewedBy { get; set; }
    public SubmittalStatus Status { get; set; }
}

public class SubmittalCreateDto
{
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubmittalStatus Status { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
}

public class SubmittalUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubmittalStatus Status { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewDate { get; set; }
    public string? Comments { get; set; }
}
