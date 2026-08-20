using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class RfiRepository : IRfiRepository
{
    private readonly ConstructionDbContext _db;
    public RfiRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<RFI> Query() => _db.RFIs.AsNoTracking();

    public Task<RFI?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.RFIs.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
}
