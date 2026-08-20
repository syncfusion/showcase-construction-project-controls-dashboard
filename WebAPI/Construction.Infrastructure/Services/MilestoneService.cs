using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProjectTaskStatus = Construction.Core.Entities.TaskStatus;

namespace Construction.Infrastructure.Services;

public class MilestoneService : IMilestoneService
{
    private readonly IMilestoneRepository _milestones;
    private readonly IProjectRepository _projects;

    public MilestoneService(IMilestoneRepository milestones, IProjectRepository projects)
    {
        _milestones = milestones;
        _projects = projects;
    }

    public async Task<PagedResponseDto<MilestoneDto>> GetMilestonesAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyFilter(_milestones.Query(), query.Filter);
        source = ApplySort(source, query.Sort);

        var total = await source.CountAsync(ct);
        var items = await source
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var projectCodes = await _projects.Query()
            .Where(p => items.Select(m => m.ProjectId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Code, ct);

        return new PagedResponseDto<MilestoneDto>
        {
            Data = items.Select(m => MapMilestone(m, projectCodes.GetValueOrDefault(m.ProjectId))).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        };
    }

    public async Task<MilestoneDto?> GetMilestoneByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _milestones.GetByIdAsync(id, ct);
        if (entity is null) return null;
        var project = await _projects.GetByIdAsync(entity.ProjectId, ct);
        return MapMilestone(entity, project?.Code);
    }

    public async Task<IReadOnlyList<MilestoneDto>> GetMilestonesByProjectAsync(int projectId, int days, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(days);
        var items = await _milestones.Query()
            .Where(m => m.ProjectId == projectId && m.Date >= DateTime.UtcNow && m.Date <= cutoff)
            .OrderBy(m => m.Date)
            .ToListAsync(ct);
        var project = await _projects.GetByIdAsync(projectId, ct);
        return items.Select(m => MapMilestone(m, project?.Code)).ToList();
    }

    public async Task<IReadOnlyList<MilestoneDto>> GetUpcomingMilestonesAsync(int days, int limit, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(days);
        var items = await _milestones.Query()
            .Where(m => m.Date >= DateTime.UtcNow && m.Date <= cutoff)
            .OrderBy(m => m.Date)
            .Take(limit)
            .ToListAsync(ct);
        var projectCodes = await _projects.Query()
            .Where(p => items.Select(m => m.ProjectId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Code, ct);
        return items.Select(m => MapMilestone(m, projectCodes.GetValueOrDefault(m.ProjectId))).ToList();
    }

    private static IQueryable<Milestone> ApplyFilter(IQueryable<Milestone> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "title" => source.Where(m => m.Name.Contains(value)),
            "status" when Enum.TryParse<ProjectTaskStatus>(value, true, out var status) => source.Where(m => m.Status == status),
            "projectid" when int.TryParse(value, out var pid) => source.Where(m => m.ProjectId == pid),
            _ => source
        };
    }

    private static IQueryable<Milestone> ApplySort(IQueryable<Milestone> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "title" => desc ? source.OrderByDescending(m => m.Name) : source.OrderBy(m => m.Name),
            "date" => desc ? source.OrderByDescending(m => m.Date) : source.OrderBy(m => m.Date),
            "status" => desc ? source.OrderByDescending(m => m.Status) : source.OrderBy(m => m.Status),
            _ => source.OrderBy(m => m.Id)
        };
    }

    private static MilestoneDto MapMilestone(Milestone m, string? projectCode = null) => new()
    {
        Id = m.Id,
        ProjectId = m.ProjectId,
        ProjectCode = projectCode ?? string.Empty,
        Title = m.Name,
        Description = m.Description,
        PlannedDate = m.Date,
        Status = m.Status,
        CreatedDate = m.CreatedDate,
        ModifiedDate = m.ModifiedDate
    };

    private static (string field, bool desc) ParseSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return ("id", false);
        var parts = sort.Split(':', 2);
        var field = parts[0].Trim().ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);
        return (field, desc);
    }
}
