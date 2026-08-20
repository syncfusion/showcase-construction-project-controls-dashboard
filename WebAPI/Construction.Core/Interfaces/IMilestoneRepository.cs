using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface IMilestoneRepository
{
    IQueryable<Milestone> Query();
    Task<Milestone?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Milestone entity, CancellationToken ct = default);
    void Update(Milestone entity);
    void Delete(Milestone entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
