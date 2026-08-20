using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface IProjectRepository
{
    IQueryable<Project> Query();
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SiteLocation>> GetSiteLocationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProjectTask>> GetProjectTasksAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<RFI>> GetProjectRfisAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<Inspection>> GetProjectInspectionsAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<Milestone>> GetProjectMilestonesAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<ChangeOrder>> GetProjectChangeOrdersAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<Budget>> GetProjectBudgetsAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetProjectDocumentsAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<Submittal>> GetProjectSubmittalsAsync(int projectId, CancellationToken ct = default);
    Task<IReadOnlyList<Risk>> GetProjectRisksAsync(int projectId, CancellationToken ct = default);
}
