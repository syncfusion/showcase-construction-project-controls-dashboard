using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repo;

    public DocumentService(IDocumentRepository repo) => _repo = repo;

    public async Task<PagedResponseDto<DocumentDto>> GetDocumentsAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyFilter(_repo.Query(), query.Filter);
        source = ApplySort(source, query.Sort);

        var total = source.Count();
        var items = source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        return await Task.FromResult(new PagedResponseDto<DocumentDto>
        {
            Data = items.Select(MapDocument).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        });
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? null : MapDocument(entity);
    }

    private static IQueryable<Document> ApplyFilter(IQueryable<Document> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "filename" => source.Where(d => d.FileName.Contains(value)),
            "filetype" => source.Where(d => d.FileType.Contains(value)),
            "documenttype" => source.Where(d => d.DocumentType != null && d.DocumentType.Contains(value)),
            "projectid" when int.TryParse(value, out var pid) => source.Where(d => d.ProjectId == pid),
            _ => source
        };
    }

    private static IQueryable<Document> ApplySort(IQueryable<Document> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "filename" => desc ? source.OrderByDescending(d => d.FileName) : source.OrderBy(d => d.FileName),
            "filetype" => desc ? source.OrderByDescending(d => d.FileType) : source.OrderBy(d => d.FileType),
            "filesize" => desc ? source.OrderByDescending(d => d.FileSize) : source.OrderBy(d => d.FileSize),
            "uploaddate" => desc ? source.OrderByDescending(d => d.UploadDate) : source.OrderBy(d => d.UploadDate),
            _ => source.OrderBy(d => d.Id)
        };
    }

    private static (string field, bool desc) ParseSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return ("id", false);
        var parts = sort.Split(':', 2);
        var field = parts[0].Trim().ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);
        return (field, desc);
    }

    private static DocumentDto MapDocument(Document d) => new()
    {
        Id = d.Id,
        ProjectId = d.ProjectId,
        FileId = d.FileId,
        FileName = d.FileName,
        FileType = d.FileType,
        FileSize = d.FileSize,
        DocumentType = d.DocumentType,
        Description = d.Description,
        UploadedBy = d.UploadedBy,
        UploadDate = d.UploadDate,
        CreatedDate = d.CreatedDate,
        ModifiedDate = d.ModifiedDate
    };
}
