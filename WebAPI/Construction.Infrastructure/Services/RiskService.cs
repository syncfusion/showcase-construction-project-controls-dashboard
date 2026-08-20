using System.Globalization;
using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Services;

public class RiskService : IRiskService
{
    private readonly IRiskRepository _risks;
    private readonly IProjectRepository _projects;

    public RiskService(IRiskRepository risks, IProjectRepository projects)
    {
        _risks = risks;
        _projects = projects;
    }

    public async Task<PagedResponseDto<RiskDto>> GetRisksAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyFilter(_risks.Query(), query.Filter);
        source = ApplySort(source, query.Sort);

        var total = await source.CountAsync(ct);

        // Join with projects to populate ProjectCode in one query
        var joined = await (
            from r in source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            join p in _projects.Query() on r.ProjectId equals p.Id into ps
            from p in ps.DefaultIfEmpty()
            select new { Risk = r, ProjectCode = p != null ? p.Code : string.Empty }
        ).ToListAsync(ct);

        return new PagedResponseDto<RiskDto>
        {
            Data = joined.Select(x => MapRisk(x.Risk, x.ProjectCode)).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        };
    }

    public async Task<RiskDto?> GetRiskByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _risks.GetByIdAsync(id, ct);
        if (entity is null) return null;

        var project = await _projects.GetByIdAsync(entity.ProjectId, ct);
        return MapRisk(entity, project?.Code ?? string.Empty);
    }

    public async Task<RiskKpisDto> GetKpisAsync(CancellationToken ct = default)
    {
        var all = await _risks.Query().ToListAsync(ct);
        var now = DateTime.UtcNow;
        return new RiskKpisDto
        {
            Critical = all.Count(r => r.Severity == RiskSeverity.Critical && r.Status != RiskStatus.Closed),
            High = all.Count(r => r.Severity == RiskSeverity.High && r.Status != RiskStatus.Closed),
            Medium = all.Count(r => r.Severity == RiskSeverity.Medium && r.Status != RiskStatus.Closed),
            Low = all.Count(r => r.Severity == RiskSeverity.Low && r.Status != RiskStatus.Closed),
            Open = all.Count(r => r.Status != RiskStatus.Closed && r.Status != RiskStatus.Mitigated),
            MitigatedThisMonth = all.Count(r => r.Status == RiskStatus.Mitigated && r.ModifiedDate?.Month == now.Month && r.ModifiedDate?.Year == now.Year)
        };
    }

    public async Task<RiskMatrixDto> GetMatrixAsync(CancellationToken ct = default)
    {
        var all = await _risks.Query().Where(r => r.Status != RiskStatus.Closed).ToListAsync(ct);

        var probabilities = new[] { RiskProbability.High, RiskProbability.Medium, RiskProbability.Low };
        var severities = new[] { RiskSeverity.Low, RiskSeverity.Medium, RiskSeverity.High, RiskSeverity.Critical };

        var rows = probabilities.Select(prob => new RiskMatrixRowDto
        {
            Probability = prob.ToString(),
            Cells = severities.Select(sev => new RiskMatrixCellDto
            {
                Severity = sev.ToString(),
                RiskNumbers = all
                    .Where(r => r.Probability == prob && r.Severity == sev)
                    .OrderBy(r => r.Number)
                    .Select(r => r.Number)
                    .ToList()
            }).ToList()
        }).ToList();

        return new RiskMatrixDto { Rows = rows };
    }

    public async Task<IReadOnlyList<RiskDto>> GetTopOpenRisksByProjectAsync(int projectId, int limit, CancellationToken ct = default)
    {
        var project = await _projects.GetByIdAsync(projectId, ct);
        var projectCode = project?.Code ?? string.Empty;

        var items = await _risks.Query()
            .Where(r => r.ProjectId == projectId && r.Status != RiskStatus.Closed)
            .OrderBy(r => r.Severity)
            .Take(limit)
            .ToListAsync(ct);
        return items.Select(r => MapRisk(r, projectCode)).ToList();
    }

    private static IQueryable<Risk> ApplyFilter(IQueryable<Risk> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "title" => source.Where(r => r.Title.Contains(value)),
            "number" => source.Where(r => r.Number.Contains(value)),
            "severity" when Enum.TryParse<RiskSeverity>(value, true, out var severity) => source.Where(r => r.Severity == severity),
            "probability" when Enum.TryParse<RiskProbability>(value, true, out var probability) => source.Where(r => r.Probability == probability),
            "status" when Enum.TryParse<RiskStatus>(value, true, out var status) => source.Where(r => r.Status == status),
            "projectid" when int.TryParse(value, out var pid) => source.Where(r => r.ProjectId == pid),
            _ => source
        };
    }

    private static IQueryable<Risk> ApplySort(IQueryable<Risk> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "number" => desc ? source.OrderByDescending(r => r.Number) : source.OrderBy(r => r.Number),
            "title" => desc ? source.OrderByDescending(r => r.Title) : source.OrderBy(r => r.Title),
            "severity" => desc ? source.OrderByDescending(r => r.Severity) : source.OrderBy(r => r.Severity),
            "probability" => desc ? source.OrderByDescending(r => r.Probability) : source.OrderBy(r => r.Probability),
            "status" => desc ? source.OrderByDescending(r => r.Status) : source.OrderBy(r => r.Status),
            "identifieddate" => desc ? source.OrderByDescending(r => r.IdentifiedDate) : source.OrderBy(r => r.IdentifiedDate),
            _ => source.OrderBy(r => r.Id)
        };
    }

    private static RiskDto MapRisk(Risk r, string? projectCode = null) => new()
    {
        Id = r.Id,
        ProjectId = r.ProjectId,
        ProjectCode = projectCode ?? string.Empty,
        Number = r.Number,
        Title = r.Title,
        Description = r.Description,
        Severity = r.Severity,
        Probability = r.Probability,
        ImpactType = r.ImpactType,
        ImpactDescription = r.ImpactDescription,
        ImpactCost = r.ImpactCost,
        ImpactDays = r.ImpactDays,
        Owner = r.Owner,
        Status = r.Status,
        MitigationPlan = r.MitigationPlan,
        IdentifiedDate = r.IdentifiedDate,
        TargetResolutionDate = r.TargetResolutionDate,
        ClosedDate = r.ClosedDate,
        CreatedDate = r.CreatedDate,
        ModifiedDate = r.ModifiedDate
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
