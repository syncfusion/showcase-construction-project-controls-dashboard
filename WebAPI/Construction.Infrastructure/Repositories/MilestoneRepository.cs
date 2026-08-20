using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class MilestoneRepository : IMilestoneRepository
{
    private readonly ConstructionDbContext _db;
    public MilestoneRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<Milestone> Query() => _db.Milestones.AsNoTracking();

    public Task<Milestone?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Milestones.AsNoTracking().FirstOrDefaultAsync(m => m.Id == m.Id, ct);

    public async Task AddAsync(Milestone entity, CancellationToken ct = default)
        => await _db.Milestones.AddAsync(entity, ct);

    public void Update(Milestone entity) => _db.Milestones.Update(entity);

    public void Delete(Milestone entity) => _db.Milestones.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
