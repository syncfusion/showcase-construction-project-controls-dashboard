using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface IRiskRepository
{
    IQueryable<Risk> Query();
    Task<Risk?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Risk entity, CancellationToken ct = default);
    void Update(Risk entity);
    void Delete(Risk entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
