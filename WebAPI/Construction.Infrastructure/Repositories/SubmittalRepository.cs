using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class SubmittalRepository : ISubmittalRepository
{
    private readonly ConstructionDbContext _db;
    public SubmittalRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<Submittal> Query() => _db.Submittals.AsNoTracking();

    public Task<Submittal?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Submittals.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
}
