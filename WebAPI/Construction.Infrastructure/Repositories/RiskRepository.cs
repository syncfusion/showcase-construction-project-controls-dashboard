using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class RiskRepository : IRiskRepository
{
    private readonly ConstructionDbContext _db;
    public RiskRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<Risk> Query() => _db.Risks.AsNoTracking();

    public Task<Risk?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Risks.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(Risk entity, CancellationToken ct = default)
        => await _db.Risks.AddAsync(entity, ct);

    public void Update(Risk entity) => _db.Risks.Update(entity);

    public void Delete(Risk entity) => _db.Risks.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
