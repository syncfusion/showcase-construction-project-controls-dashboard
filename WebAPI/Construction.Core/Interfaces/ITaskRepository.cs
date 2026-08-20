using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface ITaskRepository
{
    IQueryable<ProjectTask> Query();
    Task<ProjectTask?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(ProjectTask entity, CancellationToken ct = default);
    void Update(ProjectTask entity);
    void Delete(ProjectTask entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
