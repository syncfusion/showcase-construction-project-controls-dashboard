namespace Construction.Core.DTOs;

public class DocumentDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? DocumentType { get; set; }
    public string? Description { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime? UploadDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class DocumentCreateDto
{
    public int ProjectId { get; set; }
    public Guid FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? DocumentType { get; set; }
    public string? Description { get; set; }
    public string? UploadedBy { get; set; }
}

public class DocumentUpdateDto
{
    public string FileName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string? Description { get; set; }
}
