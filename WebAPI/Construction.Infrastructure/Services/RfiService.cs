using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Services;

public class RfiService : IRfiService
{
    private readonly IRfiRepository _repo;

    public RfiService(IRfiRepository repo) => _repo = repo;

    public async Task<PagedResponseDto<RfiDto>> GetRfisAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyFilter(_repo.Query(), query.Filter);
        source = ApplySort(source, query.Sort);

        var total = source.Count();
        var items = source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        return await Task.FromResult(new PagedResponseDto<RfiDto>
        {
            Data = items.Select(MapRfi).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        });
    }

    public async Task<RfiDto?> GetRfiByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? null : MapRfi(entity);
    }

    public async Task<IReadOnlyList<RfiSummaryDto>> GetRfisByProjectAsync(int projectId, CancellationToken ct = default)
    {
        var items = await _repo.Query()
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.SubmittedDate)
            .ToListAsync(ct);
        return items.Select(MapRfiSummary).ToList();
    }

    private static IQueryable<RFI> ApplyFilter(IQueryable<RFI> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "subject" => source.Where(r => r.Subject.Contains(value)),
            "number" => source.Where(r => r.Number.Contains(value)),
            "status" when Enum.TryParse<RFIStatus>(value, true, out var status) => source.Where(r => r.Status == status),
            "projectid" when int.TryParse(value, out var pid) => source.Where(r => r.ProjectId == pid),
            _ => source
        };
    }

    private static IQueryable<RFI> ApplySort(IQueryable<RFI> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "number" => desc ? source.OrderByDescending(r => r.Number) : source.OrderBy(r => r.Number),
            "subject" => desc ? source.OrderByDescending(r => r.Subject) : source.OrderBy(r => r.Subject),
            "submitteddate" => desc ? source.OrderByDescending(r => r.SubmittedDate) : source.OrderBy(r => r.SubmittedDate),
            "status" => desc ? source.OrderByDescending(r => r.Status) : source.OrderBy(r => r.Status),
            _ => source.OrderBy(r => r.Id)
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

    private static RfiDto MapRfi(RFI r) => new()
    {
        Id = r.Id,
        ProjectId = r.ProjectId,
        Number = r.Number,
        Subject = r.Subject,
        Description = r.Description,
        Status = r.Status,
        SubmittedBy = r.SubmittedBy,
        SubmittedDate = r.SubmittedDate,
        AssignedTo = r.AssignedTo,
        ResponseDate = r.ResponseDate,
        Response = r.Response,
        Discipline = r.Discipline,
        Impact = r.Impact,
        CreatedDate = r.CreatedDate,
        ModifiedDate = r.ModifiedDate
    };

    private static RfiSummaryDto MapRfiSummary(RFI r) => new()
    {
        Id = r.Id,
        ProjectId = r.ProjectId,
        Number = r.Number,
        Subject = r.Subject,
        Discipline = r.Discipline,
        SubmittedBy = r.SubmittedBy,
        SubmittedDate = r.SubmittedDate,
        AssignedTo = r.AssignedTo,
        Status = r.Status
    };
}
