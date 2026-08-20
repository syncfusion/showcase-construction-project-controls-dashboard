using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;

namespace Construction.Infrastructure.Services;

public class ProjectService : IProjectService
{
    // MapProject uses ReportService.ComputeHealthStatus.
    private static HealthStatus ComputeHealth(Project p) => ReportService.ComputeHealthStatus(p);

    private readonly IProjectRepository _projects;
    private readonly ITaskRepository _tasks;

    public ProjectService(IProjectRepository projects, ITaskRepository tasks)
    {
        _projects = projects;
        _tasks = tasks;
    }

    public async Task<PagedResponseDto<ProjectDto>> GetProjectsAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyProjectFilter(_projects.Query(), query.Filter);
        source = ApplyProjectSort(source, query.Sort);

        var total = source.Count();
        var items = source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        return await Task.FromResult(new PagedResponseDto<ProjectDto>
        {
            Data = items.Select(MapProject).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        });
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _projects.GetByIdAsync(id, ct);
        return entity is null ? null : MapProject(entity);
    }

    public async Task<PagedResponseDto<TaskDto>> GetProjectTasksAsync(int projectId, QueryParametersDto query, CancellationToken ct = default)
    {
        var source = _tasks.Query().Where(t => t.ProjectId == projectId);
        source = ApplyTaskFilter(source, query.Filter);
        source = ApplyTaskSort(source, query.Sort);

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

    public async Task<IReadOnlyList<MapLocationDto>> GetProjectLocationsAsync(CancellationToken ct = default)
    {
        var locations = await _projects.GetSiteLocationsAsync(ct);
        var projectGroups = locations.GroupBy(l => l.ProjectId);
        var result = new List<MapLocationDto>();

        foreach (var group in projectGroups)
        {
            var location = group.First();
            var projectId = location.ProjectId;
            var tasks = await _projects.GetProjectTasksAsync(projectId, ct);
            var rfis = await _projects.GetProjectRfisAsync(projectId, ct);
            var inspections = await _projects.GetProjectInspectionsAsync(projectId, ct);
            result.Add(MapLocation(location, tasks, rfis, inspections));
        }

        return result;
    }

    public async Task<ProjectKpisDto> GetProjectKpisAsync(int id, CancellationToken ct = default)
    {
        var project = await _projects.GetByIdAsync(id, ct);
        if (project is null) throw new ArgumentException("Project not found", nameof(id));

        var tasks = await _projects.GetProjectTasksAsync(id, ct);
        var rfis = await _projects.GetProjectRfisAsync(id, ct);
        var submittals = await _projects.GetProjectSubmittalsAsync(id, ct);
        var changeOrders = await _projects.GetProjectChangeOrdersAsync(id, ct);
        var budgets = await _projects.GetProjectBudgetsAsync(id, ct);
        var risks = await _projects.GetProjectRisksAsync(id, ct);

        var today = DateTime.UtcNow;
        var spent = budgets.Sum(b => b.SpentAmount);
        var costVariance = project.Budget > 0 ? (spent - project.Budget * (project.Progress / 100m)) / project.Budget * 100 : 0;

        var expectedProgress = project.EndDate > project.StartDate
            ? (double)(today - project.StartDate).TotalDays / (project.EndDate - project.StartDate).TotalDays * 100
            : 100;
        var scheduleVariance = project.Progress - (int)expectedProgress;

        var openRfis = rfis.Count(r => r.Status == RFIStatus.Open || r.Status == RFIStatus.UnderReview);
        var openSubmittals = submittals.Count(s => s.Status == SubmittalStatus.Submitted || s.Status == SubmittalStatus.UnderReview);

        return new ProjectKpisDto
        {
            PercentComplete = project.Progress,
            CostVariance = Math.Round(costVariance, 1),
            ScheduleVariance = scheduleVariance,
            OpenRfis = openRfis,
            OverdueRfis = rfis.Count(r => (r.Status == RFIStatus.Open || r.Status == RFIStatus.UnderReview) && r.SubmittedDate < today.AddDays(-7)),
            Budget = project.Budget,
            Spent = spent,
            OpenChangeOrders = changeOrders.Count(c => c.Status == ChangeOrderStatus.Pending),
            OpenRisks = risks.Count(r => r.Status != RiskStatus.Closed),
            OpenSubmittals = openSubmittals,
            OverdueSubmittals = submittals.Count(s => (s.Status == SubmittalStatus.Submitted || s.Status == SubmittalStatus.UnderReview) && s.SubmittedDate < today.AddDays(-21)),
            PendingChangeOrdersAmount = changeOrders.Where(c => c.Status == ChangeOrderStatus.Pending).Sum(c => c.Amount),
            ApprovedSubmittals = submittals.Count(s => s.Status == SubmittalStatus.Approved || s.Status == SubmittalStatus.ApprovedWithComments)
        };
    }

    public async Task<IReadOnlyList<RecentDocumentDto>> GetRecentDocumentsAsync(int id, int days, int limit, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var documents = await _projects.GetProjectDocumentsAsync(id, ct);
        var submittals = await _projects.GetProjectSubmittalsAsync(id, ct);
        var rfis = await _projects.GetProjectRfisAsync(id, ct);

        var result = documents
            .Where(d => d.CreatedDate >= cutoff)
            .Select(d => new RecentDocumentDto
            {
                Id = d.Id,
                DocumentNumber = d.FileName ?? $"DOC-{d.Id}",
                Title = d.Description ?? d.FileName ?? "Untitled",
                Type = d.DocumentType ?? "File",
                Revision = d.FileType,
                SubmittedDate = d.UploadDate ?? d.CreatedDate,
                Status = d.UploadDate.HasValue ? "Uploaded" : "Draft",
                ProjectId = id
            })
            .Concat(submittals.Where(s => s.SubmittedDate >= cutoff).Select(s => new RecentDocumentDto
            {
                Id = s.Id,
                DocumentNumber = s.Number,
                Title = s.Description,
                Type = "Submittal",
                Revision = null,
                SubmittedDate = s.SubmittedDate ?? s.CreatedDate,
                Status = s.Status.ToString(),
                ProjectId = id
            }))
            .Concat(rfis.Where(r => r.SubmittedDate >= cutoff).Select(r => new RecentDocumentDto
            {
                Id = r.Id,
                DocumentNumber = r.Number,
                Title = r.Subject,
                Type = "RFI",
                Revision = null,
                SubmittedDate = r.SubmittedDate ?? r.CreatedDate,
                Status = r.Status.ToString(),
                ProjectId = id
            }))
            .OrderByDescending(d => d.SubmittedDate)
            .Take(limit)
            .ToList();

        return result;
    }

    public async Task<IReadOnlyList<RiskDto>> GetTopOpenRisksAsync(int id, int limit, CancellationToken ct = default)
    {
        var risks = await _projects.GetProjectRisksAsync(id, ct);
        return risks
            .Where(r => r.Status != RiskStatus.Closed)
            .OrderBy(r => r.Severity)
            .ThenBy(r => r.IdentifiedDate)
            .Take(limit)
            .Select(r => new RiskDto
            {
                Id = r.Id,
                ProjectId = r.ProjectId,
                Number = r.Number,
                Title = r.Title,
                Severity = r.Severity,
                Probability = r.Probability,
                ImpactType = r.ImpactType,
                ImpactCost = r.ImpactCost,
                ImpactDays = r.ImpactDays,
                Owner = r.Owner,
                Status = r.Status
            })
            .ToList();
    }

    public async Task<IReadOnlyList<MilestoneDto>> GetUpcomingMilestonesAsync(int id, int days, CancellationToken ct = default)
    {
        var milestones = await _projects.GetProjectMilestonesAsync(id, ct);
        var cutoff = DateTime.UtcNow.AddDays(days);
        var project = await _projects.GetByIdAsync(id, ct);
        return milestones
            .Where(m => m.Date >= DateTime.UtcNow && m.Date <= cutoff)
            .OrderBy(m => m.Date)
            .Take(10)
            .Select(m => new MilestoneDto
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                ProjectCode = project?.Code ?? string.Empty,
                Title = m.Name,
                Description = m.Description,
                PlannedDate = m.Date,
                Status = m.Status,
                Owner = project?.Manager
            })
            .ToList();
    }

    public async Task<IReadOnlyList<RfiSummaryDto>> GetProjectRfisAsync(int id, CancellationToken ct = default)
    {
        var rfis = await _projects.GetProjectRfisAsync(id, ct);
        return rfis.Select(r => new RfiSummaryDto
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
        }).ToList();
    }

    public async Task<IReadOnlyList<SubmittalSummaryDto>> GetProjectSubmittalsAsync(int id, CancellationToken ct = default)
    {
        var submittals = await _projects.GetProjectSubmittalsAsync(id, ct);
        return submittals.Select(s => new SubmittalSummaryDto
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
        }).ToList();
    }

    public async Task<IReadOnlyList<ChangeOrderSummaryDto>> GetProjectChangeOrdersAsync(int id, CancellationToken ct = default)
    {
        var changeOrders = await _projects.GetProjectChangeOrdersAsync(id, ct);
        return changeOrders.Select(c => new ChangeOrderSummaryDto
        {
            Id = c.Id,
            ProjectId = c.ProjectId,
            Number = c.Number,
            Description = c.Description,
            Amount = c.Amount,
            Status = c.Status,
            RequestedBy = c.RequestedBy,
            RequestDate = c.RequestDate
        }).ToList();
    }

    private static IQueryable<Project> ApplyProjectFilter(IQueryable<Project> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "name" => source.Where(p => p.Name.Contains(value)),
            "code" => source.Where(p => p.Code.Contains(value)),
            "status" when Enum.TryParse<ProjectStatus>(value, true, out var status) => source.Where(p => p.Status == status),
            _ => source
        };
    }

    private static IQueryable<Project> ApplyProjectSort(IQueryable<Project> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "name" => desc ? source.OrderByDescending(p => p.Name) : source.OrderBy(p => p.Name),
            "code" => desc ? source.OrderByDescending(p => p.Code) : source.OrderBy(p => p.Code),
            "startdate" => desc ? source.OrderByDescending(p => p.StartDate) : source.OrderBy(p => p.StartDate),
            "enddate" => desc ? source.OrderByDescending(p => p.EndDate) : source.OrderBy(p => p.EndDate),
            "budget" => desc ? source.OrderByDescending(p => p.Budget) : source.OrderBy(p => p.Budget),
            _ => source.OrderBy(p => p.Id)
        };
    }

    private static IQueryable<ProjectTask> ApplyTaskFilter(IQueryable<ProjectTask> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "name" => source.Where(t => t.Name.Contains(value)),
            "status" when Enum.TryParse<Construction.Core.Entities.TaskStatus>(value, true, out var status) => source.Where(t => t.Status == status),
            _ => source
        };
    }

    private static IQueryable<ProjectTask> ApplyTaskSort(IQueryable<ProjectTask> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "name" => desc ? source.OrderByDescending(t => t.Name) : source.OrderBy(t => t.Name),
            "startdate" => desc ? source.OrderByDescending(t => t.StartDate) : source.OrderBy(t => t.StartDate),
            "enddate" => desc ? source.OrderByDescending(t => t.EndDate) : source.OrderBy(t => t.EndDate),
            "progress" => desc ? source.OrderByDescending(t => t.Progress) : source.OrderBy(t => t.Progress),
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

    private static ProjectDto MapProject(Project p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Code = p.Code,
        Description = p.Description,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = p.Status,
        Location = p.Location,
        Budget = p.Budget,
        Progress = p.Progress,
        Manager = p.Manager,
        CreatedDate = p.CreatedDate,
        ModifiedDate = p.ModifiedDate,
        HealthStatus = ComputeHealth(p)
    };

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

    private static MapLocationDto MapLocation(SiteLocation sl, IReadOnlyList<ProjectTask> tasks, IReadOnlyList<RFI> rfis, IReadOnlyList<Inspection> inspections) => new()
    {
        ProjectId = sl.ProjectId,
        Latitude = sl.Latitude,
        Longitude = sl.Longitude,
        Name = sl.Name,
        Status = sl.Project?.Status.ToString(),
        Color = GetStatusColor(sl.Project?.Status),
        StartDate = sl.Project?.StartDate,
        EndDate = sl.Project?.EndDate,
        Progress = sl.Project?.Progress ?? 0,
        Health = GetProjectHealth(sl.Project?.Status, tasks, rfis, inspections).ToString(),
        TopIssues = GetTopIssues(tasks, rfis, inspections)
    };

    private static string? GetStatusColor(ProjectStatus? status) => status switch
    {
        ProjectStatus.Active => "#34a853",
        ProjectStatus.Completed => "#0066cc",
        ProjectStatus.OnHold => "#f9ab00",
        ProjectStatus.Cancelled => "#dc3545",
        _ => "#5f6368"
    };

    private static ProjectHealth GetProjectHealth(ProjectStatus? status, IReadOnlyList<ProjectTask> tasks, IReadOnlyList<RFI> rfis, IReadOnlyList<Inspection> inspections)
    {
        var today = DateTime.UtcNow;
        var overdueTasks = tasks.Count(t => t.Status != Construction.Core.Entities.TaskStatus.Completed && t.EndDate < today);
        var openRfis = rfis.Count(r => r.Status == Construction.Core.Entities.RFIStatus.Open || r.Status == Construction.Core.Entities.RFIStatus.UnderReview);
        var failedInspections = inspections.Count(i => i.Status == Construction.Core.Entities.InspectionStatus.Failed);

        if (status == ProjectStatus.Cancelled || overdueTasks >= 5 || openRfis >= 3 || failedInspections >= 2)
            return ProjectHealth.Critical;
        if (status == ProjectStatus.OnHold || overdueTasks >= 2 || openRfis >= 1 || failedInspections >= 1)
            return ProjectHealth.AtRisk;
        return ProjectHealth.Good;
    }

    private static IReadOnlyList<string> GetTopIssues(IReadOnlyList<ProjectTask> tasks, IReadOnlyList<RFI> rfis, IReadOnlyList<Inspection> inspections)
    {
        var today = DateTime.UtcNow;
        var issues = new List<string>();

        var overdueTaskCount = tasks.Count(t => t.Status != Construction.Core.Entities.TaskStatus.Completed && t.EndDate < today);
        if (overdueTaskCount > 0)
            issues.Add($"{overdueTaskCount} overdue task{(overdueTaskCount == 1 ? "" : "s")}");

        var pendingRfiCount = rfis.Count(r => r.Status == Construction.Core.Entities.RFIStatus.Open || r.Status == Construction.Core.Entities.RFIStatus.UnderReview);
        if (pendingRfiCount > 0)
            issues.Add($"{pendingRfiCount} open RFI{(pendingRfiCount == 1 ? "" : "s")}");

        var failedInspectionCount = inspections.Count(i => i.Status == Construction.Core.Entities.InspectionStatus.Failed);
        if (failedInspectionCount > 0)
            issues.Add($"{failedInspectionCount} failed inspection{(failedInspectionCount == 1 ? "" : "s")}");

        if (issues.Count == 0)
            issues.Add("No critical issues");

        return issues.Take(3).ToList();
    }
}
