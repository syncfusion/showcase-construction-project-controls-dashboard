using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class InspectionRepository : IInspectionRepository
{
    private readonly ConstructionDbContext _db;
    public InspectionRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<Inspection> Query() => _db.Inspections.AsNoTracking();

    public Task<Inspection?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Inspections.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
}
