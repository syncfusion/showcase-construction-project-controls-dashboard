using System.Text.Json.Serialization;
using Construction.Core.Entities;

namespace Construction.Core.DTOs;

public class RfiDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RFIStatus Status { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? Response { get; set; }

    [JsonPropertyName("dueDate")]
    public DateTime? DueDate => SubmittedDate?.AddDays(14);

    public string? Discipline { get; set; }
    public string? Impact { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class RfiSummaryDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Discipline { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? DueDate => SubmittedDate?.AddDays(14);
    public string? AssignedTo { get; set; }
    public RFIStatus Status { get; set; }
}

public class RfiCreateDto
{
    public int ProjectId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RFIStatus Status { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public string? AssignedTo { get; set; }
}

public class RfiUpdateDto
{
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RFIStatus Status { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? Response { get; set; }
}
