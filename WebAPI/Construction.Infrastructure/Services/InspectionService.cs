using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Services;

public class InspectionService : IInspectionService
{
    private readonly IInspectionRepository _repo;

    public InspectionService(IInspectionRepository repo) => _repo = repo;

    public async Task<PagedResponseDto<InspectionDto>> GetInspectionsAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyFilter(_repo.Query(), query.Filter);
        source = ApplySort(source, query.Sort);

        var total = source.Count();
        var items = source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        return await Task.FromResult(new PagedResponseDto<InspectionDto>
        {
            Data = items.Select(MapInspection).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        });
    }

    public async Task<InspectionDto?> GetInspectionByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? null : MapInspection(entity);
    }

    private static IQueryable<Inspection> ApplyFilter(IQueryable<Inspection> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "type" => source.Where(i => i.Type.Contains(value)),
            "inspector" => source.Where(i => i.Inspector != null && i.Inspector.Contains(value)),
            "status" when Enum.TryParse<InspectionStatus>(value, true, out var status) => source.Where(i => i.Status == status),
            "projectid" when int.TryParse(value, out var pid) => source.Where(i => i.ProjectId == pid),
            _ => source
        };
    }

    private static IQueryable<Inspection> ApplySort(IQueryable<Inspection> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "type" => desc ? source.OrderByDescending(i => i.Type) : source.OrderBy(i => i.Type),
            "scheduleddate" => desc ? source.OrderByDescending(i => i.ScheduledDate) : source.OrderBy(i => i.ScheduledDate),
            "status" => desc ? source.OrderByDescending(i => i.Status) : source.OrderBy(i => i.Status),
            "inspector" => desc ? source.OrderByDescending(i => i.Inspector) : source.OrderBy(i => i.Inspector),
            _ => source.OrderBy(i => i.Id)
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

    private static InspectionDto MapInspection(Inspection i) => new()
    {
        Id = i.Id,
        ProjectId = i.ProjectId,
        LocationId = i.LocationId,
        Location = i.Location?.Name ?? "",
        Type = i.Type,
        ScheduledDate = i.ScheduledDate,
        CompletedDate = i.CompletedDate,
        Status = i.Status,
        Inspector = i.Inspector,
        Notes = i.Notes,
        Findings = i.Findings,
        CreatedDate = i.CreatedDate,
        ModifiedDate = i.ModifiedDate
    };
}
