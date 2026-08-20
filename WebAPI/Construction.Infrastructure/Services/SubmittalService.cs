using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Services;

public class SubmittalService : ISubmittalService
{
    private readonly ISubmittalRepository _repo;

    public SubmittalService(ISubmittalRepository repo) => _repo = repo;

    public async Task<PagedResponseDto<SubmittalDto>> GetSubmittalsAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyFilter(_repo.Query(), query.Filter);
        source = ApplySort(source, query.Sort);

        var total = source.Count();
        var items = source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        return await Task.FromResult(new PagedResponseDto<SubmittalDto>
        {
            Data = items.Select(MapSubmittal).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        });
    }

    public async Task<SubmittalDto?> GetSubmittalByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? null : MapSubmittal(entity);
    }

    public async Task<SubmittalDto> CreateSubmittalAsync(SubmittalCreateDto dto, CancellationToken ct = default)
    {
        var entity = new Submittal
        {
            ProjectId = dto.ProjectId,
            Number = dto.Number,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            SubmittedBy = dto.SubmittedBy,
            SubmittedDate = dto.SubmittedDate,
            CreatedDate = DateTime.UtcNow
        };
        return await Task.FromResult(MapSubmittal(entity));
    }

    public async Task<SubmittalDto?> UpdateSubmittalAsync(int id, SubmittalUpdateDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return null;
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Status = dto.Status;
        entity.ReviewedBy = dto.ReviewedBy;
        entity.ReviewDate = dto.ReviewDate;
        entity.Comments = dto.Comments;
        entity.ModifiedDate = DateTime.UtcNow;
        return await Task.FromResult(MapSubmittal(entity));
    }

    public async Task<bool> DeleteSubmittalAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is not null;
    }

    public async Task<IReadOnlyList<SubmittalSummaryDto>> GetSubmittalsByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var items = await _repo.Query()
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.SubmittedDate)
            .ToListAsync(ct);
        return items.Select(MapSubmittalSummary).ToList();
    }

    private static IQueryable<Submittal> ApplyFilter(IQueryable<Submittal> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "title" => source.Where(s => s.Title.Contains(value)),
            "description" => source.Where(s => s.Description.Contains(value)),
            "number" => source.Where(s => s.Number.Contains(value)),
            "status" when Enum.TryParse<SubmittalStatus>(value, true, out var status) => source.Where(s => s.Status == status),
            "projectid" when int.TryParse(value, out var pid) => source.Where(s => s.ProjectId == pid),
            _ => source
        };
    }

    private static IQueryable<Submittal> ApplySort(IQueryable<Submittal> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "number" => desc ? source.OrderByDescending(s => s.Number) : source.OrderBy(s => s.Number),
            "title" => desc ? source.OrderByDescending(s => s.Title) : source.OrderBy(s => s.Title),
            "description" => desc ? source.OrderByDescending(s => s.Description) : source.OrderBy(s => s.Description),
            "submitteddate" => desc ? source.OrderByDescending(s => s.SubmittedDate) : source.OrderBy(s => s.SubmittedDate),
            "status" => desc ? source.OrderByDescending(s => s.Status) : source.OrderBy(s => s.Status),
            _ => source.OrderBy(s => s.Id)
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

    private static SubmittalDto MapSubmittal(Submittal s) => new()
    {
        Id = s.Id,
        ProjectId = s.ProjectId,
        Number = s.Number,
        Title = s.Title,
        Description = s.Description,
        Status = s.Status,
        SubmittedBy = s.SubmittedBy,
        SubmittedDate = s.SubmittedDate,
        ReviewedBy = s.ReviewedBy,
        ReviewDate = s.ReviewDate,
        Comments = s.Comments,
        Discipline = s.Discipline,
        SpecificationSection = s.SpecificationSection,
        SubmittalType = s.SubmittalType,
        CreatedDate = s.CreatedDate,
        ModifiedDate = s.ModifiedDate
    };

    private static SubmittalSummaryDto MapSubmittalSummary(Submittal s) => new()
    {
        Id = s.Id,
        ProjectId = s.ProjectId,
        Number = s.Number,
        Title = s.Title,
        Description = s.Description,
        Discipline = s.Discipline,
        SubmittalType = s.SubmittalType,
        SubmittedBy = s.SubmittedBy,
        SubmittedDate = s.SubmittedDate,
        ReviewedBy = s.ReviewedBy,
        Status = s.Status
    };
}
