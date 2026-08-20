using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ConstructionDbContext _db;
    public ProjectRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<Project> Query() => _db.Projects.AsNoTracking();

    public Task<Project?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<SiteLocation>> GetSiteLocationsAsync(CancellationToken ct = default)
        => await _db.SiteLocations
            .AsNoTracking()
            .Include(sl => sl.Project)
            .OrderBy(sl => sl.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProjectTask>> GetProjectTasksAsync(int projectId, CancellationToken ct = default)
        => await _db.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.EndDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RFI>> GetProjectRfisAsync(int projectId, CancellationToken ct = default)
        => await _db.RFIs
            .AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.SubmittedDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Inspection>> GetProjectInspectionsAsync(int projectId, CancellationToken ct = default)
        => await _db.Inspections
            .AsNoTracking()
            .Where(i => i.ProjectId == projectId)
            .OrderBy(i => i.ScheduledDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Milestone>> GetProjectMilestonesAsync(int projectId, CancellationToken ct = default)
        => await _db.Milestones
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.Date)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ChangeOrder>> GetProjectChangeOrdersAsync(int projectId, CancellationToken ct = default)
        => await _db.ChangeOrders
            .AsNoTracking()
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.RequestDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Budget>> GetProjectBudgetsAsync(int projectId, CancellationToken ct = default)
        => await _db.Budgets
            .AsNoTracking()
            .Where(b => b.ProjectId == projectId)
            .Include(b => b.CostItems)
            .OrderBy(b => b.Category)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Document>> GetProjectDocumentsAsync(int projectId, CancellationToken ct = default)
        => await _db.Documents
            .AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Submittal>> GetProjectSubmittalsAsync(int projectId, CancellationToken ct = default)
        => await _db.Submittals
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.SubmittedDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Risk>> GetProjectRisksAsync(int projectId, CancellationToken ct = default)
        => await _db.Risks
            .AsNoTracking()
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.Number)
            .ToListAsync(ct);
}
