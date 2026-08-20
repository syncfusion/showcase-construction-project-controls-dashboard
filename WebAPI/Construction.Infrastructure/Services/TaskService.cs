using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;

    public TaskService(ITaskRepository tasks)
    {
        _tasks = tasks;
    }

    public async Task<PagedResponseDto<TaskDto>> GetTasksAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyFilter(_tasks.Query(), query.Filter);
        source = ApplySort(source, query.Sort);

        var total = source.Count();
        var items = source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        return await Task.FromResult(new PagedResponseDto<TaskDto>
        {
            Data = items.Select(MapTask).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        });
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _tasks.GetByIdAsync(id, ct);
        return entity is null ? null : MapTask(entity);
    }

    private static IQueryable<ProjectTask> ApplyFilter(IQueryable<ProjectTask> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "name" => source.Where(t => t.Name.Contains(value)),
            "projectid" when int.TryParse(value, out var pid) => source.Where(t => t.ProjectId == pid),
            "status" when Enum.TryParse<Construction.Core.Entities.TaskStatus>(value, true, out var status) => source.Where(t => t.Status == status),
            _ => source
        };
    }

    private static IQueryable<ProjectTask> ApplySort(IQueryable<ProjectTask> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "name" => desc ? source.OrderByDescending(t => t.Name) : source.OrderBy(t => t.Name),
            "startdate" => desc ? source.OrderByDescending(t => t.StartDate) : source.OrderBy(t => t.StartDate),
            "enddate" => desc ? source.OrderByDescending(t => t.EndDate) : source.OrderBy(t => t.EndDate),
            "progress" => desc ? source.OrderByDescending(t => t.Progress) : source.OrderBy(t => t.Progress),
            "projectid" => desc ? source.OrderByDescending(t => t.ProjectId) : source.OrderBy(t => t.ProjectId),
            _ => source.OrderBy(t => t.Id)
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

    private static TaskDto MapTask(ProjectTask t) => new()
    {
        Id = t.Id,
        ProjectId = t.ProjectId,
        Name = t.Name,
        Description = t.Description,
        StartDate = t.StartDate,
        EndDate = t.EndDate,
        Status = t.Status,
        Progress = t.Progress,
        AssignedTo = t.AssignedTo,
        ParentTaskId = t.ParentTaskId,
        Dependencies = t.Dependencies,
        CreatedDate = t.CreatedDate,
        ModifiedDate = t.ModifiedDate
    };
}
