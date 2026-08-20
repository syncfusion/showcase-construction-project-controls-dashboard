using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ConstructionDbContext _db;
    public TaskRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<ProjectTask> Query() => _db.Tasks.AsNoTracking();

    public Task<ProjectTask?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(ProjectTask entity, CancellationToken ct = default)
        => await _db.Tasks.AddAsync(entity, ct);

    public void Update(ProjectTask entity) => _db.Tasks.Update(entity);

    public void Delete(ProjectTask entity) => _db.Tasks.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
